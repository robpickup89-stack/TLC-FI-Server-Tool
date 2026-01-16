using System.Text.Json;
using TLCFI.Models;

namespace TLCFI.Server;

public sealed class SimulatorStateStore
{
    private readonly Dictionary<ObjectKey, JsonElement> _meta = new();
    private readonly Dictionary<ObjectKey, JsonElement> _state = new();

    public void SetMeta(int type, string id, JsonElement payload) => _meta[new ObjectKey(type, id)] = payload;
    public void SetState(int type, string id, JsonElement payload) => _state[new ObjectKey(type, id)] = payload;

    public IReadOnlyDictionary<ObjectKey, JsonElement> Meta => _meta;
    public IReadOnlyDictionary<ObjectKey, JsonElement> State => _state;
}
