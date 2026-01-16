using System.Text;

namespace TLCFiTool.Transport;

public sealed class StreamReadLoop
{
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly NdjsonFramer _framer = new();

    public StreamReadLoop(Stream stream, Encoding? encoding = null)
    {
        _stream = stream;
        _encoding = encoding ?? Encoding.UTF8;
    }

    public async Task ReadAsync(Func<string, Task> onMessage, CancellationToken token)
    {
        var buffer = new byte[4096];
        while (!token.IsCancellationRequested)
        {
            var bytesRead = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
            if (bytesRead == 0)
            {
                break;
            }

            var text = _encoding.GetString(buffer, 0, bytesRead);
            foreach (var line in _framer.Feed(text))
            {
                await onMessage(line);
            }
        }
    }
}
