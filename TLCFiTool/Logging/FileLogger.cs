namespace TLCFiTool.Logging;

public sealed class FileLogger
{
    private readonly string _path;

    public FileLogger(string path)
    {
        _path = path;
    }

    public async Task LogAsync(string message)
    {
        var line = $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}";
        await File.AppendAllTextAsync(_path, line);
    }
}
