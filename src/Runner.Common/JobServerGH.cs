using GitHub.DistributedTask.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Services.WebApi;

namespace GitHub.Runner.Common
{
    public sealed class JobServerGH : RunnerService, IJobServer
    {
        // private bool _hasConnection;
        // private VssConnection _connection;
        // private TaskHttpClient _taskClient;

        public Task ConnectAsync(VssConnection jobConnection)
        {
            // _connection = jobConnection;
            // int attemptCount = 5;
            // while (!_connection.HasAuthenticated && attemptCount-- > 0)
            // {
            //     try
            //     {
            //         await _connection.ConnectAsync();
            //         break;
            //     }
            //     catch (Exception ex) when (attemptCount > 0)
            //     {
            //         Trace.Info($"Catch exception during connect. {attemptCount} attemp left.");
            //         Trace.Error(ex);
            //     }
            //
            //     await Task.Delay(100);
            // }
            //
            // _taskClient = _connection.GetClient<TaskHttpClient>();
            // _hasConnection = true;

            // Always connected!
            return Task.CompletedTask;
        }

        private void CheckConnection()
        {
            // if (!_hasConnection)
            // {
            //     throw new InvalidOperationException("SetConnection");
            // }
        }

        //-----------------------------------------------------------------
        // Feedback: WebConsole, TimelineRecords and Logs
        //-----------------------------------------------------------------

        public Task<TaskLog> AppendLogContentAsync(Guid scopeIdentifier, string hubName, Guid planId, int logId, Stream uploadStream, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.AppendLogContentAsync(scopeIdentifier, hubName, planId, logId, uploadStream, cancellationToken: cancellationToken);

            return Task.FromResult(new TaskLog(hubName));
        }

        public Task AppendTimelineRecordFeedAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, Guid timelineRecordId, Guid stepId, IList<string> lines, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.AppendTimelineRecordFeedAsync(scopeIdentifier, hubName, planId, timelineId, timelineRecordId, stepId, lines, cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }

        public Task AppendTimelineRecordFeedAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, Guid timelineRecordId, Guid stepId, IList<string> lines, long startLine, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.AppendTimelineRecordFeedAsync(scopeIdentifier, hubName, planId, timelineId, timelineRecordId, stepId, lines, startLine, cancellationToken: cancellationToken);
            
            return Task.CompletedTask;
        }

        public Task<TaskAttachment> CreateAttachmentAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, Guid timelineRecordId, string type, string name, Stream uploadStream, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.CreateAttachmentAsync(scopeIdentifier, hubName, planId, timelineId, timelineRecordId, type, name, uploadStream, cancellationToken: cancellationToken);

            return Task.FromResult(new TaskAttachment(type, name));
        }

        public Task<TaskLog> CreateLogAsync(Guid scopeIdentifier, string hubName, Guid planId, TaskLog log, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.CreateLogAsync(scopeIdentifier, hubName, planId, log, cancellationToken: cancellationToken);

            return Task.FromResult(log);
        }

        public Task<Timeline> CreateTimelineAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.CreateTimelineAsync(scopeIdentifier, hubName, planId, new Timeline(timelineId), cancellationToken: cancellationToken);

            this.Trace.Verbose($"Creating timeline {timelineId}");
            
            // We are never calling this for GH actions.
            return Task.FromResult(new Timeline(timelineId));
        }

        public Task<List<TimelineRecord>> UpdateTimelineRecordsAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, IEnumerable<TimelineRecord> records, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.UpdateTimelineRecordsAsync(scopeIdentifier, hubName, planId, timelineId, records, cancellationToken: cancellationToken);
            
            foreach (var timelineRecord in records) 
            {
                this.Trace.Verbose($"Update TimelineRecord {timelineRecord.Name} {timelineRecord.State} {timelineRecord.Result}");
            }

            return Task.FromResult(new List<TimelineRecord>());
        }

        public async Task RaisePlanEventAsync<T>(Guid scopeIdentifier, string hubName, Guid planId, T eventData, CancellationToken cancellationToken) where T : JobEvent
        {
            // CheckConnection();
            // return _taskClient.RaisePlanEventAsync(scopeIdentifier, hubName, planId, eventData, cancellationToken: cancellationToken);
            
            this.Trace.Info($"RaisePlanEventAsync: {eventData.Name}");

            using var httpClient = new HttpClient();
            using var result =
                await httpClient.PostAsync(new Uri($"http://localhost:5014/events?job_id={eventData.JobId}&signal_name={eventData.Name}"), null);
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("Could not sent event");
            }

            this.Trace.Info($"RaisedPlanEventAsync: {eventData.Name} {eventData.JobId}");
        }

        public Task<Timeline> GetTimelineAsync(Guid scopeIdentifier, string hubName, Guid planId, Guid timelineId, CancellationToken cancellationToken)
        {
            //     CheckConnection();
            //     return _taskClient.GetTimelineAsync(scopeIdentifier, hubName, planId, timelineId, includeRecords: true, cancellationToken: cancellationToken);

            return Task.FromResult(new Timeline(timelineId));
        }

        //-----------------------------------------------------------------
        // Action download info
        //-----------------------------------------------------------------
        public Task<ActionDownloadInfoCollection> ResolveActionDownloadInfoAsync(Guid scopeIdentifier, string hubName, Guid planId, ActionReferenceList actions, CancellationToken cancellationToken)
        {
            // CheckConnection();
            // return _taskClient.ResolveActionDownloadInfoAsync(scopeIdentifier, hubName, planId, actions, cancellationToken: cancellationToken);

            return Task.FromResult(new ActionDownloadInfoCollection());
        }
    }
}
