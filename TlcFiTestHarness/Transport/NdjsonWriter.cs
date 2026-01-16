using System.Text;
using System.Threading.Channels;

namespace TLCFI.Transport;

public sealed class NdjsonWriter : IAsyncDisposable
{
    private readonly Channel<string> _queue = Channel.CreateUnbounded<string>();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _pumpTask;
    private readonly Stream _stream;

    public NdjsonWriter(Stream stream)
    {
        _stream = stream;
        _pumpTask = Task.Run(PumpAsync);
    }

    public ValueTask EnqueueAsync(string json, CancellationToken cancellationToken = default)
    {
        return _queue.Writer.WriteAsync(json, cancellationToken);
    }

    private async Task PumpAsync()
    {
        var encoder = Encoding.UTF8;
        try
        {
            while (await _queue.Reader.WaitToReadAsync(_cts.Token).ConfigureAwait(false))
            {
                while (_queue.Reader.TryRead(out var message))
                {
                    var payload = encoder.GetBytes(message + "\n");
                    await _stream.WriteAsync(payload, _cts.Token).ConfigureAwait(false);
                    await _stream.FlushAsync(_cts.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _queue.Writer.TryComplete();
        await _pumpTask.ConfigureAwait(false);
        _cts.Dispose();
    }
}
