using System.Text.Json;
using TLCFiTool.JsonRpc;

namespace TLCFiTool.Client;

public static class ProtocolHelpers
{
    public static string BuildRequest(string id, string method, object? parameters)
    {
        var message = new JsonRpcMessage
        {
            Id = id,
            Method = method,
            Params = parameters,
        };

        return JsonSerializer.Serialize(message);
    }
}
