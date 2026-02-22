namespace HDRFixer.Core.Ipc;

public enum IpcMessageType { Request, Response, Notification, Error }

public class IpcMessage
{
    public IpcMessageType Type { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    public static IpcMessage CreateRequest(string action, string? payload = null)
        => new() { Type = IpcMessageType.Request, Action = action, Payload = payload };

    public static IpcMessage CreateResponse(IpcMessage request, string? payload = null)
        => new() { Type = IpcMessageType.Response, Action = request.Action, RequestId = request.RequestId, Payload = payload };
}
