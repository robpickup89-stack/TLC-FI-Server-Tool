using System.Text.Json;

namespace TLCFI.JsonRpc;

public sealed record RpcError(int Code, string Message)
{
    public JsonElement ToJson() => JsonSerializer.SerializeToElement(new { code = Code, message = Message });

    public static RpcError IncorrectCredentials() => new(1, "Incorrect credentials");
    public static RpcError NotRegistered() => new(2, "Not registered");
    public static RpcError PermissionDenied() => new(3, "Permission denied");
}
