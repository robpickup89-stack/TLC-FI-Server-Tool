using System.Text.Json;
using System.Text.Json.Serialization;
using TLCFiTool.JsonRpc;
using TLCFiTool.Models;

namespace TLCFiTool.Auth;

public sealed class RegistrationHandler
{
    private readonly PasswordAuth _auth;

    public RegistrationHandler(PasswordAuth auth)
    {
        _auth = auth;
    }

    public Task<JsonRpcMessage> HandleAsync(JsonRpcMessage request, Session session)
    {
        var parameters = request.Params as JsonElement?;
        if (parameters is null)
        {
            return Task.FromResult(BuildFailure(request.Id));
        }

        RegisterParams? payload;
        try
        {
            payload = JsonSerializer.Deserialize<RegisterParams>(parameters.Value);
        }
        catch (JsonException)
        {
            return Task.FromResult(BuildFailure(request.Id));
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Username) || string.IsNullOrWhiteSpace(payload.Password))
        {
            return Task.FromResult(BuildFailure(request.Id));
        }

        if (!Enum.IsDefined(typeof(ApplicationType), payload.Type))
        {
            return Task.FromResult(BuildFailure(request.Id));
        }

        var appType = (ApplicationType)payload.Type;
        var authResult = _auth.Authenticate(payload.Username, payload.Password, appType);
        if (!authResult.Authenticated)
        {
            return Task.FromResult(BuildFailure(request.Id));
        }

        session.IsRegistered = true;
        session.Identity = authResult.Username ?? payload.Username;
        session.Username = authResult.Username ?? payload.Username;
        session.ApplicationType = appType;
        session.Version = payload.Version;
        session.Revision = payload.Revision;
        session.ClientUri = payload.Uri ?? string.Empty;
        session.LastSeen = DateTimeOffset.UtcNow;

        return Task.FromResult(new JsonRpcMessage
        {
            Id = request.Id,
            Result = new RegisterResult
            {
                Accepted = true,
                SessionId = $"S{session.SessionId:N}",
                Type = payload.Type,
                Version = payload.Version,
                Revision = payload.Revision,
            },
        });
    }

    private static JsonRpcMessage BuildFailure(string? id)
    {
        return new JsonRpcMessage
        {
            Id = id,
            Error = new JsonRpcError { Code = 1, Message = "Incorrect credentials" },
        };
    }

    private sealed class RegisterParams
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("revision")]
        public int Revision { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }
    }

    private sealed class RegisterResult
    {
        [JsonPropertyName("accepted")]
        public bool Accepted { get; set; }

        [JsonPropertyName("sessionid")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("revision")]
        public int Revision { get; set; }
    }
}
