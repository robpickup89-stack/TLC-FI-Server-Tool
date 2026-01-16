using TLCFI.Models;
using TLCFI.Storage;

namespace TLCFI.Server;

public sealed class AuthService
{
    private readonly AppStorage _storage;
    private UserDatabase _db = new();

    public AuthService(AppStorage storage)
    {
        _storage = storage;
    }

    public async Task InitializeAsync()
    {
        var defaultDb = new UserDatabase
        {
            Users =
            [
                new UserRecord { Username = "Jason", Password = "<set_me>", AllowedTypes = [2] },
                new UserRecord { Username = "admin", Password = "<set_me>", AllowedTypes = [0, 1, 2] }
            ]
        };
        _db = await _storage.LoadAsync(_storage.UsersPath, defaultDb).ConfigureAwait(false);
    }

    public UserRecord? Validate(string username, string password, int type)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        if (user is null || !user.Password.Equals(password, StringComparison.Ordinal))
        {
            return null;
        }

        if (!user.AllowedTypes.Contains(type))
        {
            return null;
        }

        return user;
    }
}
