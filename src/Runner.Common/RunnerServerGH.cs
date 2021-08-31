using GitHub.DistributedTask.WebApi;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Runner.Common.Util;
using GitHub.Services.WebApi;
using GitHub.Services.Common;
using GitHub.Runner.Sdk;

namespace GitHub.Runner.Common
{
    public sealed class RunnerServerGH : RunnerService, IRunnerServer
    {
        public Task ConnectAsync(Uri serverUrl, VssCredentials credentials)
        {
            // var createGenericConnection = EstablishVssConnection(serverUrl, credentials, TimeSpan.FromSeconds(100));
            // var createMessageConnection = EstablishVssConnection(serverUrl, credentials, TimeSpan.FromSeconds(60));
            // var createRequestConnection = EstablishVssConnection(serverUrl, credentials, TimeSpan.FromSeconds(60));
            //
            // await Task.WhenAll(createGenericConnection, createMessageConnection, createRequestConnection);
            //
            // _genericConnection = await createGenericConnection;
            // _messageConnection = await createMessageConnection;
            // _requestConnection = await createRequestConnection;
            //
            // _genericTaskAgentClient = _genericConnection.GetClient<TaskAgentHttpClient>();
            // _messageTaskAgentClient = _messageConnection.GetClient<TaskAgentHttpClient>();
            // _requestTaskAgentClient = _requestConnection.GetClient<TaskAgentHttpClient>();
            //
            // _hasGenericConnection = true;
            // _hasMessageConnection = true;
            // _hasRequestConnection = true;
            
            return Task.CompletedTask;
        }

        // Refresh connection is best effort. it should never throw exception
        public Task RefreshConnectionAsync(RunnerConnectionType connectionType, TimeSpan timeout)
        {
            // Trace.Info($"Refresh {connectionType} VssConnection to get on a different AFD node.");
            // VssConnection newConnection = null;
            // switch (connectionType)
            // {
            //     case RunnerConnectionType.MessageQueue:
            //         try
            //         {
            //             _hasMessageConnection = false;
            //             newConnection = await EstablishVssConnection(_messageConnection.Uri, _messageConnection.Credentials, timeout);
            //             var client = newConnection.GetClient<TaskAgentHttpClient>();
            //             _messageConnection = newConnection;
            //             _messageTaskAgentClient = client;
            //         }
            //         catch (Exception ex)
            //         {
            //             Trace.Error($"Catch exception during reset {connectionType} connection.");
            //             Trace.Error(ex);
            //             newConnection?.Dispose();
            //         }
            //         finally
            //         {
            //             _hasMessageConnection = true;
            //         }
            //         break;
            //     case RunnerConnectionType.JobRequest:
            //         try
            //         {
            //             _hasRequestConnection = false;
            //             newConnection = await EstablishVssConnection(_requestConnection.Uri, _requestConnection.Credentials, timeout);
            //             var client = newConnection.GetClient<TaskAgentHttpClient>();
            //             _requestConnection = newConnection;
            //             _requestTaskAgentClient = client;
            //         }
            //         catch (Exception ex)
            //         {
            //             Trace.Error($"Catch exception during reset {connectionType} connection.");
            //             Trace.Error(ex);
            //             newConnection?.Dispose();
            //         }
            //         finally
            //         {
            //             _hasRequestConnection = true;
            //         }
            //         break;
            //     case RunnerConnectionType.Generic:
            //         try
            //         {
            //             _hasGenericConnection = false;
            //             newConnection = await EstablishVssConnection(_genericConnection.Uri, _genericConnection.Credentials, timeout);
            //             var client = newConnection.GetClient<TaskAgentHttpClient>();
            //             _genericConnection = newConnection;
            //             _genericTaskAgentClient = client;
            //         }
            //         catch (Exception ex)
            //         {
            //             Trace.Error($"Catch exception during reset {connectionType} connection.");
            //             Trace.Error(ex);
            //             newConnection?.Dispose();
            //         }
            //         finally
            //         {
            //             _hasGenericConnection = true;
            //         }
            //         break;
            //     default:
            //         Trace.Error($"Unexpected connection type: {connectionType}.");
            //         break;
            // }
            
            return Task.CompletedTask;
        }

        public void SetConnectionTimeout(RunnerConnectionType connectionType, TimeSpan timeout)
        {
            // Trace.Info($"Set {connectionType} VssConnection's timeout to {timeout.TotalSeconds} seconds.");
            // switch (connectionType)
            // {
            //     case RunnerConnectionType.JobRequest:
            //         _requestConnection.Settings.SendTimeout = timeout;
            //         break;
            //     case RunnerConnectionType.MessageQueue:
            //         _messageConnection.Settings.SendTimeout = timeout;
            //         break;
            //     case RunnerConnectionType.Generic:
            //         _genericConnection.Settings.SendTimeout = timeout;
            //         break;
            //     default:
            //         Trace.Error($"Unexpected connection type: {connectionType}.");
            //         break;
            // }
        }

        //-----------------------------------------------------------------
        // Configuration
        //-----------------------------------------------------------------

        public Task<List<TaskAgentPool>> GetAgentPoolsAsync(string agentPoolName = null, TaskAgentPoolType poolType = TaskAgentPoolType.Automation)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.GetAgentPoolsAsync(agentPoolName, poolType: poolType);

            return Task.FromResult(new List<TaskAgentPool>());
        }

        public Task<TaskAgent> AddAgentAsync(Int32 agentPoolId, TaskAgent agent)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.AddAgentAsync(agentPoolId, agent);

            return Task.FromResult(agent);
        }

        public Task<List<TaskAgent>> GetAgentsAsync(int agentPoolId, string agentName = null)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.GetAgentsAsync(agentPoolId, agentName, false);

            return Task.FromResult(new List<TaskAgent>());
        }

        public Task<TaskAgent> ReplaceAgentAsync(int agentPoolId, TaskAgent agent)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.ReplaceAgentAsync(agentPoolId, agent);
            
            return Task.FromResult(agent);
        }

        public Task DeleteAgentAsync(int agentPoolId, int agentId)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.DeleteAgentAsync(agentPoolId, agentId);

            return Task.CompletedTask;
        }

        //-----------------------------------------------------------------
        // MessageQueue
        //-----------------------------------------------------------------

        public Task<TaskAgentSession> CreateAgentSessionAsync(Int32 poolId, TaskAgentSession session, CancellationToken cancellationToken)
        {
            CheckConnection(RunnerConnectionType.MessageQueue);
            return _messageTaskAgentClient.CreateAgentSessionAsync(poolId, session, cancellationToken: cancellationToken);
        }

        public Task DeleteAgentMessageAsync(Int32 poolId, Int64 messageId, Guid sessionId, CancellationToken cancellationToken)
        {
            // CheckConnection(RunnerConnectionType.MessageQueue);
            // return _messageTaskAgentClient.DeleteMessageAsync(poolId, messageId, sessionId, cancellationToken: cancellationToken);

            return Task.CompletedTask;
        }

        public Task DeleteAgentSessionAsync(Int32 poolId, Guid sessionId, CancellationToken cancellationToken)
        {
            // CheckConnection(RunnerConnectionType.MessageQueue);
            // return _messageTaskAgentClient.DeleteAgentSessionAsync(poolId, sessionId, cancellationToken: cancellationToken); 

            return Task.CompletedTask;
        }

        public async Task<TaskAgentMessage> GetAgentMessageAsync(Int32 poolId, Guid sessionId, Int64? lastMessageId, CancellationToken cancellationToken)
        {
            // CheckConnection(RunnerConnectionType.MessageQueue);
            // return _messageTaskAgentClient.GetMessageAsync(poolId, sessionId, lastMessageId, cancellationToken: cancellationToken);

            var httpClient = new HttpClient();

            var result = await httpClient.GetAsync(new Uri($"http://localhost:5014/messages/{sessionId}"));
            
        }

        //-----------------------------------------------------------------
        // JobRequest
        //-----------------------------------------------------------------

        public Task<TaskAgentJobRequest> RenewAgentRequestAsync(int poolId, long requestId, Guid lockToken, string orchestrationId = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            // CheckConnection(RunnerConnectionType.JobRequest);
            // return _requestTaskAgentClient.RenewAgentRequestAsync(poolId, requestId, lockToken, orchestrationId: orchestrationId, cancellationToken: cancellationToken);

            return Task.FromResult(new TaskAgentJobRequest()
            {
                // TODO: Ignore this concept for now for all practical purposes
                LockedUntil = DateTime.UtcNow + TimeSpan.FromHours(4)
            });
        }

        public Task<TaskAgentJobRequest> FinishAgentRequestAsync(int poolId, long requestId, Guid lockToken, DateTime finishTime, TaskResult result, CancellationToken cancellationToken = default(CancellationToken))
        {
            // CheckConnection(RunnerConnectionType.JobRequest);
            // return _requestTaskAgentClient.FinishAgentRequestAsync(poolId, requestId, lockToken, finishTime, result, cancellationToken: cancellationToken);

            return Task.FromResult(new TaskAgentJobRequest());
        }

        public Task<TaskAgentJobRequest> GetAgentRequestAsync(int poolId, long requestId, CancellationToken cancellationToken = default(CancellationToken))
        {
            // CheckConnection(RunnerConnectionType.JobRequest);
            // return _requestTaskAgentClient.GetAgentRequestAsync(poolId, requestId, cancellationToken: cancellationToken);
            
            // This should be enough for now
            return Task.FromResult(new TaskAgentJobRequest());  
        }

        //-----------------------------------------------------------------
        // Agent Package
        //-----------------------------------------------------------------
        public Task<List<PackageMetadata>> GetPackagesAsync(string packageType, string platform, int top, bool includeToken, CancellationToken cancellationToken)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.GetPackagesAsync(packageType, platform, top, includeToken, cancellationToken: cancellationToken);
            
            throw new NotImplementedException();
        }

        public Task<PackageMetadata> GetPackageAsync(string packageType, string platform, string version, bool includeToken, CancellationToken cancellationToken)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.GetPackageAsync(packageType, platform, version, includeToken, cancellationToken: cancellationToken);
            
            throw new NotImplementedException();
        }

        public Task<TaskAgent> UpdateAgentUpdateStateAsync(int agentPoolId, int agentId, string currentState)
        {
            // CheckConnection(RunnerConnectionType.Generic);
            // return _genericTaskAgentClient.UpdateAgentUpdateStateAsync(agentPoolId, agentId, currentState);
            throw new NotImplementedException();
        }
    }
}
