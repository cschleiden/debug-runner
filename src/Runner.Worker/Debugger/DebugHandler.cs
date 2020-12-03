using System;
using System.Threading.Tasks;
using GitHub.Runner.Common;
using GitHub.Runner.Worker;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace Runner.Worker.Debugger
{
    [ServiceLocator(Default = typeof(NoopDebugHandler))]
    public interface IDebugHandler : IRunnerService
    {
        Task Stop();
        
        Task BeforeStep(int stepIndex, IExecutionContext jobContext, IStep step);

        Task AfterStep(string step);

        void WriteLog(string message);
    }

    public class NoopDebugHandler : IDebugHandler
    {
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
    
    public class DebugHandler : RunnerService, IDebugHandler
    {
        private IDebugAdapter adapter;

        public override void Initialize(IHostContext hostContext)
        {
            base.Initialize(hostContext);

            this.adapter = this.HostContext.GetService<IDebugAdapter>();
            // adapter.Protocol.LogMessage += (sender, e) => Console.WriteLine(e.Message);
        }

        public Task Stop()
        {
            this.adapter.Stop();
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
    }
}