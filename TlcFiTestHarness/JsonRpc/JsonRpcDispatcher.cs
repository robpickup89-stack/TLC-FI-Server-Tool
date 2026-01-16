using System.Text.Json;

namespace TLCFI.JsonRpc;

public sealed class JsonRpcDispatcher
{
    public event Func<RpcMessage, Task<JsonElement?>>? RequestReceived;
    public event Action<RpcMessage>? NotificationReceived;

    public async Task<JsonElement?> DispatchAsync(RpcMessage message)
    {
        if (message.Method is not null && message.Id is not null)
        {
            if (RequestReceived is null)
            {
                return JsonSerializer.SerializeToElement(new { });
            }

            return await RequestReceived.Invoke(message).ConfigureAwait(false);
        }

        if (message.Method is not null)
        {
            NotificationReceived?.Invoke(message);
        }

        return null;
    }
}
