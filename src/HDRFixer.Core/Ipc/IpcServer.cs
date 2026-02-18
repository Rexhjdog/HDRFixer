using System.IO.Pipes;
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
            using var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try
            {
                await server.WaitForConnectionAsync(ct);
                using var reader = new StreamReader(server, Encoding.UTF8);
                using var writer = new StreamWriter(server, Encoding.UTF8) { AutoFlush = true };
                string? line = await reader.ReadLineAsync(ct);
                if (line != null)
                {
                    var message = JsonSerializer.Deserialize<IpcMessage>(line);
                    if (message != null)
                    {
                        MessageReceived?.Invoke(message);
                        var response = new IpcMessage { Type = IpcMessageType.Response, Action = message.Action, RequestId = message.RequestId };
                        await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (IOException) { }
        }
    }

    public void Stop() => _cts?.Cancel();
    public void Dispose() { _cts?.Cancel(); _cts?.Dispose(); }
}
