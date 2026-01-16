using System.Text.Json;
using TLCFI.Models;

namespace TLCFI.Client;

public sealed class ClientStateStore
{
    private readonly Dictionary<ObjectKey, JsonElement> _meta = new();
    private readonly Dictionary<ObjectKey, JsonElement> _state = new();

    public void SetMeta(int type, string id, JsonElement meta) => _meta[new ObjectKey(type, id)] = meta;
    public void SetState(int type, string id, JsonElement state) => _state[new ObjectKey(type, id)] = state;

    public IReadOnlyDictionary<ObjectKey, JsonElement> Meta => _meta;
    public IReadOnlyDictionary<ObjectKey, JsonElement> State => _state;
}
