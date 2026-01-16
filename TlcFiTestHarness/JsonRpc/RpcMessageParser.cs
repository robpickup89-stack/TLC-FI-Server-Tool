using System.Text.Json;

namespace TLCFI.JsonRpc;

public sealed class RpcMessageParser
{
    public RpcMessage Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement.Clone();
        var method = root.TryGetProperty("method", out var methodProp) ? methodProp.GetString() : null;
        var id = root.TryGetProperty("id", out var idProp) ? idProp.Clone() : (JsonElement?)null;
        var @params = root.TryGetProperty("params", out var paramsProp) ? paramsProp.Clone() : (JsonElement?)null;
        var result = root.TryGetProperty("result", out var resultProp) ? resultProp.Clone() : (JsonElement?)null;
        var error = root.TryGetProperty("error", out var errorProp) ? errorProp.Clone() : (JsonElement?)null;
        return new RpcMessage(root, method, id, @params, result, error);
    }
}

public sealed record RpcMessage(JsonElement Root, string? Method, JsonElement? Id, JsonElement? Params, JsonElement? Result, JsonElement? Error);
