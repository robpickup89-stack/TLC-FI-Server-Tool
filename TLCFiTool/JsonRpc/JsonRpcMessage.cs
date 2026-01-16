using System.Text.Json.Serialization;

namespace TLCFiTool.JsonRpc;

public sealed class JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public string Version { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public object? Params { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }
}
