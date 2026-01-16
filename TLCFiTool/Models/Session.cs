namespace TLCFiTool.Models;

public sealed class Session
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public string Identity { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public ApplicationType ApplicationType { get; set; }
    public bool IsRegistered { get; set; }
    public int Version { get; set; }
    public int Revision { get; set; }
    public string ClientUri { get; set; } = string.Empty;
    public ControlState ControlState { get; set; } = ControlState.NotConfigured;
    public string RequestedIntersection { get; set; } = string.Empty;
    public ControlState? RequestedControlState { get; set; }
    public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.UtcNow;
}
