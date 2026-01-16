using System.Text.Json;

namespace TLCFiTool.JsonRpc;

public sealed class JsonRpcDispatcher
{
    private readonly Dictionary<string, Func<JsonRpcMessage, Task<JsonRpcMessage>>> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public void Register(string method, Func<JsonRpcMessage, Task<JsonRpcMessage>> handler)
    {
        _handlers[method] = handler;
    }

    public async Task<JsonRpcMessage> DispatchAsync(JsonRpcMessage request)
    {
        if (request.Method is null)
        {
            return new JsonRpcMessage
            {
                Id = request.Id,
                Error = new JsonRpcError { Code = -32600, Message = "Invalid Request" },
            };
        }

        if (_handlers.TryGetValue(request.Method, out var handler))
        {
            return await handler(request);
        }

        return new JsonRpcMessage
        {
            Id = request.Id,
            Error = new JsonRpcError { Code = -32601, Message = "Method not found" },
        };
    }
}
