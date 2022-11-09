using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitHub.DistributedTask.ObjectTemplating;
using GitHub.DistributedTask.ObjectTemplating.Tokens;
using GitHub.DistributedTask.Pipelines;
using GitHub.DistributedTask.Pipelines.ContextData;
using GitHub.DistributedTask.Pipelines.ObjectTemplating;
using GitHub.Runner.Common;
using GitHub.Runner.Common.Util;
using GitHub.Runner.Sdk;
using GitHub.Runner.Worker.Handlers;
using Runner.Worker.Debugger;
using Pipelines = GitHub.DistributedTask.Pipelines;

namespace GitHub.Runner.Worker
{
    [ServiceLocator(Default = typeof(DebugRunner))]
    public interface IDebugRunner : IStep, IRunnerService
    {
    }

    public class DebugRunner : RunnerService, IDebugRunner
    {
        public String Condition { get; set; } = $"{Constants.Expressions.Always}()";
        public TemplateToken ContinueOnError { get; }
        public String DisplayName { get; set; } = "Waiting for debugger";
        public IExecutionContext ExecutionContext { get; set; }
        public TemplateToken Timeout { get; }
        public Task RunAsync()
        {


            return Task.CompletedTask;
        }
    }
}
