using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Runner.Common;
using GitHub.Runner.Worker;

namespace Runner.Worker.Debugger
{
  [ServiceLocator(Default = typeof(DebugServer))]
  public interface IDebugServer : IRunnerService
  {
    Task<bool> WaitForConnection(IExecutionContext context, object data);

    Task<string> GetTaskDisplayName();
  }

  public class DebugServer : RunnerService, IDebugServer
  {
    private const int TimeoutSeconds = 120;
    
    private IDebugAdapter _adapter;

    public async Task<bool> WaitForConnection(IExecutionContext context, object _)
    {
      // TODO: Use external address here? 
      var localAddr = IPAddress.Parse(Address);
      var server = new TcpListener(localAddr, Port);

      var connected = false;
      
      var cts = new CancellationTokenSource();
      cts.CancelAfter(TimeoutSeconds * 1_000);
      var cancellationToken = cts.Token;

      TcpClient client = null;
      
      await using (cancellationToken.Register(() =>
      {
        // ReSharper disable once AccessToModifiedClosure
        if (!connected)
        {
          context.Output($"No connection within {TimeoutSeconds} seconds, skipping.");
          server.Stop();
        }
      }))
      {
        server.Start();

        context.Debug("Waiting for connection");

        try
        {
          client = await server.AcceptTcpClientAsync();
          context.Debug("Accepted connection");

          connected = true;
        }
        catch
        {
          // Ignore error
        }
      }

      if (connected && client != null)
      {
        var stream = client.GetStream();
        this._adapter = this.HostContext.GetService<IDebugAdapter>();

        context.Debug("Starting debug adapter");
        await this._adapter.Run(stream, stream);
          
        // Override the default NoopHandler registration
        this.HostContext.SetServiceType<IDebugHandler, DebugHandler>();
        
        // Override the default paginger logger implementation
        this.HostContext.SetServiceType<IPagingLogger, DebugLogger>();
      }

      return connected;
    }

    public Task<string> GetTaskDisplayName()
    {
      return Task.FromResult($"Debugger waiting on: {Address}:{Port}");
    }

    // TODO: We need to figure out how to get the external IP here
    private string Address => "127.0.0.1";

    private int Port => 41085;
  }
}