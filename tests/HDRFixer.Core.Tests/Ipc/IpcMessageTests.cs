using System.Text.Json;
using HDRFixer.Core.Ipc;
using Xunit;

namespace HDRFixer.Core.Tests.Ipc;

public class IpcMessageTests
{
    [Fact]
    public void SerializesCommand()
    {
        var msg = new IpcMessage { Type = IpcMessageType.Command, Action = "ApplyFix",
            Payload = JsonSerializer.Serialize(new { FixName = "Gamma" }) };
        var d = JsonSerializer.Deserialize<IpcMessage>(JsonSerializer.Serialize(msg))!;
        Assert.Equal(IpcMessageType.Command, d.Type);
        Assert.Equal("ApplyFix", d.Action);
    }

    [Fact]
    public void SerializesEvent()
    {
        var msg = new IpcMessage { Type = IpcMessageType.Event, Action = "FixApplied" };
        Assert.Contains("FixApplied", JsonSerializer.Serialize(msg));
    }

    [Fact]
    public void AllMessageTypes() => Assert.Equal(4, Enum.GetValues<IpcMessageType>().Length);
}
