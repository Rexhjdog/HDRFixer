namespace HDRFixer.Core.Ipc;

public enum IpcMessageType { Request, Response, Notification, Error }

public class IpcMessage
{
    public IpcMessageType Type { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
}
