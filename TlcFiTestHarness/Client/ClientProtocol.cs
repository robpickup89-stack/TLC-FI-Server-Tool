using System.Text.Json;
using TLCFI.Models;

namespace TLCFI.Client;

public sealed class ClientProtocol
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public string BuildRegister(string id, string username, string password, int type, AppVersion version, string uri)
    {
        var payload = new
        {
            method = "Register",
            @params = new
            {
                username,
                password,
                type,
                version = new { major = version.Major, minor = version.Minor, revision = version.Revision },
                uri
            },
            id,
            jsonrpc = "2.0"
        };

        return JsonSerializer.Serialize(payload, _options);
    }

    public string BuildDeregister(string id, string username)
    {
        var payload = new
        {
            method = "Deregister",
            @params = new { username },
            id,
            jsonrpc = "2.0"
        };

        return JsonSerializer.Serialize(payload, _options);
    }

    public string BuildReadMeta(string id, int type, IEnumerable<string> ids)
    {
        var payload = new
        {
            method = "ReadMeta",
            @params = new { type, ids = ids.ToArray() },
            id,
            jsonrpc = "2.0"
        };

        return JsonSerializer.Serialize(payload, _options);
    }

    public string BuildSubscribe(string id, int type, IEnumerable<string> ids)
    {
        var payload = new
        {
            method = "Subscribe",
            @params = new { type, ids = ids.ToArray() },
            id,
            jsonrpc = "2.0"
        };

        return JsonSerializer.Serialize(payload, _options);
    }
}
