using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using GitHub.Runner.Common;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace GitHub.Runner.Worker
{
    [ServiceLocator(Default = typeof(NoopDebugHandler))]
    public interface IDebugHandler : IRunnerService
    {
        Task Run();

        Task Stop();
        
        Task BeforeStep(int stepIndex, IExecutionContext jobContext, IStep step);

        Task AfterStep(string step);

        void WriteLog(string message);
    }

    public class NoopDebugHandler : IDebugHandler
    {
        public Task Run()
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
        
        public Task BeforeStep(int stepIndex, IExecutionContext jobContext, IStep step)
        {
            return Task.CompletedTask;
        }

        public Task AfterStep(string step)
        {
            return Task.CompletedTask;
        }

        public void WriteLog(string message)
        {
        }

        public void Initialize(IHostContext context)
        {
        }
    }
    
    public class DebugHandler : IDebugHandler
    {
        private DebugAdapter adapter;

        public DebugHandler()
        {
            this.adapter = new DebugAdapter(Console.OpenStandardInput(), Console.OpenStandardOutput());
            // adapter.Protocol.LogMessage += (sender, e) => Console.WriteLine(e.Message);
        }

        public Task Run()
        {
            return adapter.Run();
        }

        public Task Stop()
        {
            this.adapter.Protocol.SendEvent(new ExitedEvent(exitCode: 0));
            this.adapter.Protocol.SendEvent(new TerminatedEvent());
            this.adapter.Protocol.Stop();
            this.adapter.Protocol.WaitForReader();
            return Task.CompletedTask;
        }
        
        public Task BeforeStep(int stepIndex, IExecutionContext jobContext, IStep step)
        {
            return this.adapter.Break(stepIndex, jobContext, step);
        }

        public Task AfterStep(string step)
        {
            return Task.CompletedTask;
        }

        public void WriteLog(string message)
        {
            this.adapter.Log(message);
        }

        public void Initialize(IHostContext context)
        {
        }
    }
}