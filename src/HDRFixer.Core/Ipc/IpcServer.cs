using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace HDRFixer.Core.Ipc;

public class IpcServer : IDisposable
{
    private const string PipeName = "HDRFixer_Service";
    private CancellationTokenSource? _cts;

    public event Action<IpcMessage>? MessageReceived;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _ = ListenLoop(_cts.Token);
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                NamedPipeServerStream server;

                if (OperatingSystem.IsWindows())
                {
                    var pipeSecurity = new PipeSecurity();
                    pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
                    pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
                    pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));

                    server = NamedPipeServerStreamAcl.Create(
                        PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous,
                        0,
                        0,
                        pipeSecurity);
                }
                else
                {
                    server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
                }

                await server.WaitForConnectionAsync(ct);
                _ = HandleConnectionAsync(server, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception)
            {
                if (!ct.IsCancellationRequested)
                    await Task.Delay(1000, ct);
            }
        }
    }

    private async Task HandleConnectionAsync(NamedPipeServerStream server, CancellationToken ct)
    {
        using (server)
        {
            try
            {
                using var reader = new StreamReader(server, Encoding.UTF8);
                using var writer = new StreamWriter(server, Encoding.UTF8) { AutoFlush = true };

                while (!ct.IsCancellationRequested && server.IsConnected)
                {
                    string? line = await reader.ReadLineAsync(ct);
                    if (line == null) break;

                    try
                    {
                        var message = JsonSerializer.Deserialize<IpcMessage>(line);
                        if (message != null)
                        {
                            MessageReceived?.Invoke(message);

                            var response = new IpcMessage
                            {
                                Type = IpcMessageType.Response,
                                Action = message.Action,
                                RequestId = message.RequestId,
                                Payload = "Success"
                            };
                            await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                        }
                    }
                    catch (JsonException) { }
                }
            }
            catch (IOException) { }
            catch (OperationCanceledException) { }
        }
    }

    public void Stop() => _cts?.Cancel();
    public void Dispose() { _cts?.Cancel(); _cts?.Dispose(); }
}
