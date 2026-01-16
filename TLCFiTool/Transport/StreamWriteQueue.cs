using System.Text;
using System.Threading.Channels;

namespace TLCFiTool.Transport;

public sealed class StreamWriteQueue
{
    private readonly Stream _stream;
    private readonly Encoding _encoding;
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

    public StreamWriteQueue(Stream stream, Encoding? encoding = null)
    {
        _stream = stream;
        _encoding = encoding ?? Encoding.UTF8;
    }

    public async Task EnqueueAsync(string message, CancellationToken token)
    {
        await _channel.Writer.WriteAsync(message, token);
    }

    public async Task RunAsync(CancellationToken token)
    {
        await foreach (var message in _channel.Reader.ReadAllAsync(token))
        {
            var payload = _encoding.GetBytes(message + "\n");
            await _stream.WriteAsync(payload, token);
            await _stream.FlushAsync(token);
        }
    }
}
