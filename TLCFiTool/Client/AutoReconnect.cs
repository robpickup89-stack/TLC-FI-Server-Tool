namespace TLCFiTool.Client;

public sealed class AutoReconnect
{
    public async Task RunAsync(Func<CancellationToken, Task> connectAction, CancellationToken token)
    {
        var delay = TimeSpan.FromSeconds(1);
        while (!token.IsCancellationRequested)
        {
            try
            {
                await connectAction(token);
                delay = TimeSpan.FromSeconds(1);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(delay, token);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
            }
        }
    }
}
