using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace HDRFixer.Core.Ipc;

public class IpcClient
{
    private const string PipeName = "HDRFixer_Service";

    public async Task<IpcMessage?> SendCommandAsync(IpcMessage command, CancellationToken ct = default)
    {
        using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await client.ConnectAsync(5000, ct);
        using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
        using var reader = new StreamReader(client, Encoding.UTF8);
        await writer.WriteLineAsync(JsonSerializer.Serialize(command));
        string? responseLine = await reader.ReadLineAsync(ct);
        return responseLine == null ? null : JsonSerializer.Deserialize<IpcMessage>(responseLine);
    }
}
