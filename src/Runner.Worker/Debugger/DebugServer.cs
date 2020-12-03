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
    Task<bool> WaitForConnection(IExecutionContext context, int timeoutSeconds);
  }

  public class DebugServer : RunnerService, IDebugServer
  {
    private IDebugAdapter _adapter;

    public async Task<bool> WaitForConnection(IExecutionContext context, int timeoutSeconds)
    {
      // TODO: Use external address here? 
      var localAddr = IPAddress.Parse("127.0.0.1");
      var port = 41085;
      var server = new TcpListener(localAddr, port);
      
      context.Output($"Waiting for connection at {localAddr.ToString()}:{port}");

      var connected = false;
      
      var cts = new CancellationTokenSource();
      cts.CancelAfter(timeoutSeconds * 1_000);
      var cancellationToken = cts.Token;

      TcpClient client = null;
      
      await using (cancellationToken.Register(() =>
      {
        // ReSharper disable once AccessToModifiedClosure
        if (!connected)
        {
          context.Output($"No connection within {timeoutSeconds} seconds, skipping.");
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
      }

      return connected;
    }
  }
}