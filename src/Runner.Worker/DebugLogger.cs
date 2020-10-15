using System;
using GitHub.Runner.Common;

namespace GitHub.Runner.Worker
{
    
    public class DebugLogger : IPagingLogger
    {
        private IHostContext _context;

        public void Initialize(IHostContext context)
        {
            this._context = context;
        }

        public long TotalLines { get; private set; }
        
        public void Setup(Guid timelineId, Guid timelineRecordId)
        {
        }

        public void Write(string message)
        {
            string line = $"{DateTime.UtcNow.ToString("O")} {message}";
            var handler = this._context.GetService<IDebugHandler>();
            handler.WriteLog(message);

            this.TotalLines++;
            if (line.IndexOf('\n') != -1)
            {
                foreach (char c in line)
                {
                    if (c == '\n')
                    {
                        this.TotalLines++;
                    }
                }
            }
        }

        public void End()
        {
        }
    }
}