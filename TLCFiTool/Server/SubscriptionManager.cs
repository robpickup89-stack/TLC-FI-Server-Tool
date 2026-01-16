namespace TLCFiTool.Server;

public sealed class SubscriptionManager
{
    private readonly Dictionary<string, HashSet<string>> _subscriptions = new(StringComparer.OrdinalIgnoreCase);

    public void ReplaceSubscription(string objectType, IEnumerable<string> ids)
    {
        _subscriptions[objectType] = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsSubscribed(string objectType, string id)
    {
        return _subscriptions.TryGetValue(objectType, out var set) && set.Contains(id);
    }

    public IReadOnlyDictionary<string, HashSet<string>> Snapshot() => _subscriptions;
}
