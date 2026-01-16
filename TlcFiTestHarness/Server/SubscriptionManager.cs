namespace TLCFI.Server;

public sealed class SubscriptionManager
{
    private readonly Dictionary<Guid, Dictionary<int, HashSet<string>>> _subscriptions = new();

    public void UpdateSubscription(Guid clientId, int type, IEnumerable<string> ids)
    {
        if (!_subscriptions.TryGetValue(clientId, out var perType))
        {
            perType = new Dictionary<int, HashSet<string>>();
            _subscriptions[clientId] = perType;
        }

        perType[type] = new HashSet<string>(ids);
    }

    public bool IsSubscribed(Guid clientId, int type, string id)
    {
        if (!_subscriptions.TryGetValue(clientId, out var perType))
        {
            return false;
        }

        return perType.TryGetValue(type, out var ids) && ids.Contains(id);
    }

    public IReadOnlyDictionary<int, HashSet<string>>? GetSubscriptions(Guid clientId)
    {
        return _subscriptions.TryGetValue(clientId, out var perType) ? perType : null;
    }

    public void Remove(Guid clientId) => _subscriptions.Remove(clientId);
}
