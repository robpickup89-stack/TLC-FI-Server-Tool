using System.Text.Json.Serialization;

namespace TLCFiTool.JsonRpc;

public sealed class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    public static JsonRpcError PermissionDenied() => new() { Code = -32010, Message = "Permission denied" };
}
