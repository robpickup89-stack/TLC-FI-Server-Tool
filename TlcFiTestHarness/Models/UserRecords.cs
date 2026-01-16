namespace TLCFI.Models;

public sealed class UserRecord
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<int> AllowedTypes { get; set; } = [];
}

public sealed class UserDatabase
{
    public List<UserRecord> Users { get; set; } = [];
}
