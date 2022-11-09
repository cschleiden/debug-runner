using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitHub.DistributedTask.Expressions2.Sdk;
using GitHub.DistributedTask.ObjectTemplating;
using GitHub.DistributedTask.ObjectTemplating.Tokens;
using GitHub.DistributedTask.Pipelines.ContextData;
using GitHub.DistributedTask.WebApi;
using GitHub.Runner.Common;
using GitHub.Runner.Worker;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Protocol;

namespace Runner.Worker.Debugger
{
    [ServiceLocator(Default = typeof(DebugAdapter))]
    public interface IDebugAdapter : IRunnerService
    {
        Task Run(Stream input, Stream output);
        Task Stop();
        void Log(string message);
        Task Break(int stepIdx, IExecutionContext jobContext, IStep step, bool force = false);
    }

    public class DebugAdapter : DebugAdapterBase, IDebugAdapter
    {
        private HashSet<int> breakpoints = new HashSet<int>();

        private TaskCompletionSource<bool> _taskCompletionSource = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _breakpointCompletionSource = new TaskCompletionSource<bool>();
        private IExecutionContext _jobContext;
        private IStep _step;
        private int _stepIndex;

        public void Initialize(IHostContext context)
        {
            this.HostContext = context;
        }

        private IHostContext HostContext { get; set; }

        public Task Run(Stream input, Stream output)
        {
            base.InitializeProtocolClient(input, output, DebugProtocolOptions.None);

            base.Protocol.RequestReceived += (sender, args) =>
            {
                Console.WriteLine($"Received command: {args.Command}");
            };

            this.Protocol.DispatcherError += (sender, args) =>
            {
                this._taskCompletionSource.TrySetResult(true);
                this._breakpointCompletionSource?.TrySetResult(true);
            };

            this.Protocol.Run();

            return this._taskCompletionSource.Task;
        }

        public Task Stop()
        {
            this.Protocol.SendEvent(new TerminatedEvent());
            this.Protocol.Stop();
            this.Protocol.WaitForReader();

            return Task.CompletedTask;
        }

        public void Log(string message)
        {
            this.Protocol.SendEvent(new OutputEvent(message));
        }

        public Task Break(int stepIdx, IExecutionContext jobContext, IStep step, bool force = false)
        {
            if (!this.Protocol.IsRunning || (!force && !this.breakpoints.Contains(stepIdx)))
            {
                // Continue
                return Task.CompletedTask;
            }

            this._jobContext = jobContext;
            this._step = step;
            this._stepIndex = stepIdx;
            this._breakpointCompletionSource = new TaskCompletionSource<bool>();

            // Break
            this.Protocol.SendEvent(new StoppedEvent()
            {
                Reason = StoppedEvent.ReasonValue.Breakpoint,
                Description = "Paused at step",
                AllThreadsStopped = true
            });

            this.Log($"Stopped at step {stepIdx}");

            // Prevent deadlocks during development
            return Task.WhenAny(this._breakpointCompletionSource.Task, Task.Delay(TimeSpan.FromMinutes(1)));
        }

        protected override AttachResponse HandleAttachRequest(AttachArguments arguments)
        {
            return new AttachResponse();
        }

        protected override InitializeResponse HandleInitializeRequest(InitializeArguments arguments)
        {
            this.Protocol.SendEvent(new InitializedEvent());

            return new InitializeResponse()
            {
                SupportsModulesRequest = false
            };
        }

        protected override SetBreakpointsResponse HandleSetBreakpointsRequest(SetBreakpointsArguments arguments)
        {
            // Add step indexes to list of breakpoints
            arguments.Breakpoints.ForEach(x => this.breakpoints.Add(x.Line));

            return new SetBreakpointsResponse()
            {
                Breakpoints = arguments.Breakpoints.Select(x => new Breakpoint()
                {
                    Line = x.Line
                }).ToList()
            };
        }

        protected override StackTraceResponse HandleStackTraceRequest(StackTraceArguments arguments)
        {
            return new StackTraceResponse()
            {
                TotalFrames = 2,
                StackFrames = new List<StackFrame>()
                {
                    new StackFrame()
                    {
                        Id = 0,
                        Name = this._step.DisplayName,
                        Column = 0,
                        Line = this._stepIndex, // Use the Line field to transmit the idx, the adapter will map it back to the actual line number
                        Source = new Source() // The client will fill this in
                        {
                        }
                    },
                    new StackFrame()
                    {
                        Id = 1,
                        Name = "Job", // TODO: Use job name here?
                        Column = 0,
                        Line = 0,
                        Source = new Source()
                        {
                        }
                    }
                }
            };
        }

        // This will grow and grow... need to reset on stop maybe?
        private List<KeyValuePair<string, PipelineContextData>> varReferences = new List<KeyValuePair<string, PipelineContextData>>();

        private int topLevelOffset = 100;

        protected override VariablesResponse HandleVariablesRequest(VariablesArguments arguments)
        {
            try
            {
                if (arguments.VariablesReference == 1)
                {
                    // 1 are the top-level keys for the step
                    return new VariablesResponse()
                    {
                        Variables = this._step.ExecutionContext.ExpressionValues.Keys
                            .Select((k, idx) =>
                            {
                                this.varReferences.Add(new KeyValuePair<string, PipelineContextData>(k,
                                    this._step.ExecutionContext.ExpressionValues[k]));

                                return new Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable(k,
                                    "Object", this.topLevelOffset + this.varReferences.Count - 1);
                            })
                            .ToList()
                    };
                }
                else
                {
                    var v = this.varReferences[arguments.VariablesReference - this.topLevelOffset];
                    if (v.Value is IReadOnlyObject dict)
                    {
                        return new VariablesResponse()
                        {
                            Variables = dict.Keys
                                .Select((k, idx) =>
                                {
                                    var value = dict[k];

                                    if (value is DictionaryContextData || value is CaseSensitiveDictionaryContextData)
                                    {
                                        this.varReferences.Add(
                                            new KeyValuePair<string, PipelineContextData>(k,
                                                dict[k] as PipelineContextData));
                                        return new Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable(
                                            k,
                                            "Object",
                                            this.varReferences.Count - 1 + this.topLevelOffset);
                                    }
                                    else
                                    {
                                        return new Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable(
                                            k,
                                            value?.ToString() ?? "null",
                                            0);
                                    }

                                })
                                .ToList()
                        };
                    }
                    else
                    {
                        return new VariablesResponse()
                        {
                            Variables = new List<Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable>()
                            {
                                new Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable(
                                    v.Key,
                                    v.Value?.ToString() ?? "null",
                                    0)
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                this.Log(ex.ToString());

                return new VariablesResponse()
                {
                    Variables = new List<Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages.Variable>()
                };
            }
        }

        protected override EvaluateResponse HandleEvaluateRequest(EvaluateArguments arguments)
        {
            if (this._step == null)
            {
                return new EvaluateResponse();
            }

            try
            {
                var evaluator = this._step.ExecutionContext.ToPipelineTemplateEvaluator();
                var value = evaluator.EvaluateStepDisplayName(
                    new BasicExpressionToken(null, null, null, expression: arguments.Expression),
                    this._step.ExecutionContext.ExpressionValues,
                    this._step.ExecutionContext.ExpressionFunctions);

                return new EvaluateResponse(value, 0);
            }
            catch
            {
                return new EvaluateResponse("Could not evaluate expression", 0);
            }
        }

        protected override ConfigurationDoneResponse HandleConfigurationDoneRequest(ConfigurationDoneArguments arguments)
        {
            this.ConfigurationDone();

            return new ConfigurationDoneResponse();
        }

        protected override ContinueResponse HandleContinueRequest(ContinueArguments arguments)
        {
            this._breakpointCompletionSource.SetResult(true);

            return new ContinueResponse()
            {
                AllThreadsContinued = true
            };
        }

        protected override TerminateResponse HandleTerminateRequest(TerminateArguments arguments)
        {
            this._breakpointCompletionSource?.SetResult(true);

            this.Stop();

            return new TerminateResponse();
        }

        protected override DisconnectResponse HandleDisconnectRequest(DisconnectArguments arguments)
        {
            this._breakpointCompletionSource?.SetResult(true);

            this.Protocol.Stop();

            return new DisconnectResponse();
        }

        private void ConfigurationDone()
        {
            this._taskCompletionSource.SetResult(true);
        }
    }
}
