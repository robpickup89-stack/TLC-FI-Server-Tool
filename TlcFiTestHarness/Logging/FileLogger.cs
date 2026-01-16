namespace TLCFI.Logging;

public sealed class FileLogger
{
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileLogger(string path)
    {
        _path = path;
    }

    public async Task AppendAsync(string message)
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            await File.AppendAllTextAsync(_path, message + Environment.NewLine).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }
}
