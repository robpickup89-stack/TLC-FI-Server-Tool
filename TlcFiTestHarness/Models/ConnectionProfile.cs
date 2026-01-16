namespace TLCFI.Models;

public sealed class ConnectionProfile
{
    public string Name { get; set; } = "Default";
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 11501;
    public string Username { get; set; } = "Chameleon";
    public string Password { get; set; } = "CHAM2";
    public int Type { get; set; } = 1;
    public AppVersion Version { get; set; } = new(1, 1, 0);
    public string Uri { get; set; } = string.Empty;
    public bool AutoDiscover { get; set; } = true;
    public bool AutoSubscribe { get; set; } = true;
}
