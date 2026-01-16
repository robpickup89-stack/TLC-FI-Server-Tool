namespace TLCFI.Logging;

public sealed class UiLogger
{
    private readonly Action<string> _sink;

    public UiLogger(Action<string> sink)
    {
        _sink = sink;
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {level} {message}";
        _sink(line);
    }
}
