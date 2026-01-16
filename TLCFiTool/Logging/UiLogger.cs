namespace TLCFiTool.Logging;

public sealed class UiLogger
{
    public event Action<string>? Message;

    public void Log(string message)
    {
        Message?.Invoke($"[{DateTimeOffset.Now:HH:mm:ss}] {message}");
    }
}
