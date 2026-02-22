using System.Text.Json;
using HDRFixer.Core.Ipc;
using Xunit;

namespace HDRFixer.Core.Tests.Ipc;

public class IpcMessageTests
{
    [Fact]
    public void SerializesRequest()
    {
        var msg = new IpcMessage { Type = IpcMessageType.Request, Action = "ApplyFix",
            Payload = JsonSerializer.Serialize(new { FixName = "Gamma" }) };
        var json = JsonSerializer.Serialize(msg);
        var d = JsonSerializer.Deserialize<IpcMessage>(json)!;
        Assert.Equal(IpcMessageType.Request, d.Type);
        Assert.Equal("ApplyFix", d.Action);
    }

    [Fact]
    public void SerializesNotification()
    {
        var msg = new IpcMessage { Type = IpcMessageType.Notification, Action = "FixApplied" };
        Assert.Contains("FixApplied", JsonSerializer.Serialize(msg));
    }

    [Fact]
    public void AllMessageTypes() => Assert.Equal(4, Enum.GetValues<IpcMessageType>().Length);
}
