using System.Text.Json;
using TLCFI.Models;

namespace TLCFI.Storage;

public sealed class AppStorage
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public string RootPath { get; }

    public AppStorage(string appName = "TlcFiTestHarness")
    {
        RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
        Directory.CreateDirectory(RootPath);
        Directory.CreateDirectory(Path.Combine(RootPath, "scripts"));
    }

    public string UsersPath => Path.Combine(RootPath, "users.json");
    public string ProfilesPath => Path.Combine(RootPath, "profiles.json");
    public string ConfigPath => Path.Combine(RootPath, "config.json");
    public string ScriptsPath => Path.Combine(RootPath, "scripts");

    public async Task<T> LoadAsync<T>(string path, T fallback)
    {
        if (!File.Exists(path))
        {
            await SaveAsync(path, fallback).ConfigureAwait(false);
            return fallback;
        }

        var json = await File.ReadAllTextAsync(path).ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(json, _options) ?? fallback;
    }

    public Task SaveAsync<T>(string path, T data)
    {
        var json = JsonSerializer.Serialize(data, _options);
        return File.WriteAllTextAsync(path, json);
    }
}
