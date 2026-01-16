namespace TLCFiTool.Core;

public sealed class TickService : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private long _ticks;

    public TickService(double intervalMs = 100)
    {
        _timer = new System.Timers.Timer(intervalMs);
        _timer.Elapsed += (_, _) => Tick?.Invoke(Interlocked.Increment(ref _ticks));
    }

    public event Action<long>? Tick;

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    public void Dispose() => _timer.Dispose();
}
