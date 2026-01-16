using System.Text.Json;
using TLCFiTool.Models;

namespace TLCFiTool.Auth;

public sealed class PasswordAuth
{
    private readonly List<UserRecord> _users = new();

    public IReadOnlyList<UserRecord> Users => _users;

    public async Task LoadUsersAsync(string path)
    {
        if (!File.Exists(path))
        {
            _users.Clear();
            _users.AddRange(UserRecord.DefaultUsers());
            await SaveUsersAsync(path);
            return;
        }

        await using var stream = File.OpenRead(path);
        var payload = await JsonSerializer.DeserializeAsync<UserPayload>(stream) ?? new UserPayload();
        _users.Clear();
        _users.AddRange(payload.Users);
    }

    public async Task SaveUsersAsync(string path)
    {
        var payload = new UserPayload { Users = _users };
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, payload, new JsonSerializerOptions { WriteIndented = true });
    }

    public AuthResult Authenticate(string username, string password, ApplicationType requestedType)
    {
        var user = _users.FirstOrDefault(entry => string.Equals(entry.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            return AuthResult.Fail("Invalid username");
        }

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return AuthResult.Fail("Invalid password");
        }

        if (!user.AllowedApplicationTypes.Contains(requestedType))
        {
            return AuthResult.Fail("Application type not allowed");
        }

        return AuthResult.Success(Guid.NewGuid().ToString("N"), user.Username);
    }

    public sealed class UserRecord
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<ApplicationType> AllowedApplicationTypes { get; set; } = new();

        public static IEnumerable<UserRecord> DefaultUsers() => new[]
        {
            new UserRecord { Username = "Jason", Password = "<set_me>", AllowedApplicationTypes = new List<ApplicationType> { ApplicationType.Control } },
            new UserRecord { Username = "admin", Password = "<set_me>", AllowedApplicationTypes = new List<ApplicationType> { ApplicationType.Provider, ApplicationType.Consumer, ApplicationType.Control } },
        };
    }

    public sealed class AuthResult
    {
        private AuthResult(bool authenticated, string? token, string message, string? username)
        {
            Authenticated = authenticated;
            Token = token;
            Message = message;
            Username = username;
        }

        public bool Authenticated { get; }
        public string? Token { get; }
        public string Message { get; }
        public string? Username { get; }

        public static AuthResult Success(string token, string username) => new(true, token, "Authenticated", username);

        public static AuthResult Fail(string message) => new(false, null, message, null);
    }

    private sealed class UserPayload
    {
        public List<UserRecord> Users { get; set; } = new();
    }
}
