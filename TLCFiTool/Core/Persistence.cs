using System.Text.Json;
using System.Text.Json.Serialization;
using TLCFiTool.Models;

namespace TLCFiTool.Core;

public sealed class Persistence
{
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task SaveConfigAsync(string path, StateStore stateStore)
    {
        var payload = new
        {
            detectors = stateStore.Detectors,
            outputs = stateStore.Outputs,
            intersections = stateStore.Intersections,
        };

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, payload, _options);
    }

    public async Task<StateStore> LoadConfigAsync(string path)
    {
        var store = new StateStore();
        if (!File.Exists(path))
        {
            store.SeedDefaults(255, 255);
            return store;
        }

        await using var stream = File.OpenRead(path);
        var payload = await JsonSerializer.DeserializeAsync<ConfigPayload>(stream, _options) ?? new ConfigPayload();
        store.Detectors.AddRange(payload.Detectors);
        store.Outputs.AddRange(payload.Outputs);
        store.Intersections.AddRange(payload.Intersections);
        return store;
    }

    private sealed class ConfigPayload
    {
        public List<Detector> Detectors { get; set; } = new();
        public List<Output> Outputs { get; set; } = new();
        public List<Intersection> Intersections { get; set; } = new();
    }
}
