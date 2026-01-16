using System.Text.Json;
using TLCFI.Models;

namespace TLCFI.Server;

public static class SimulatorDefaults
{
    public static SimulatorConfig Build()
    {
        var config = new SimulatorConfig
        {
            Intersections = [new SimulatorItem { Id = "SIM_INT_1", Index = 1 }],
            SignalGroups = Enumerable.Range(0, 9).Select(i => new SimulatorItem { Id = ((char)('A' + i)).ToString(), Index = i + 1 }).ToList(),
            Detectors = Enumerable.Range(1, 255).Select(i => new SimulatorItem { Id = $"DET{i}", Index = i }).ToList(),
            Outputs = Enumerable.Range(1, 255).Select(i => new SimulatorItem { Id = $"OUT{i}", Index = i }).ToList(),
            Inputs = Enumerable.Range(1, 32).Select(i => new SimulatorItem { Id = $"IN{i}", Index = i }).ToList(),
            Variables = Enumerable.Range(1, 32).Select(i => new SimulatorItem { Id = $"VAR{i}", Index = i }).ToList()
        };

        return config;
    }

    public static void ApplyDefaults(SimulatorConfig config, SimulatorStateStore store)
    {
        foreach (var detector in config.Detectors)
        {
            store.SetMeta(4, detector.Id, JsonSerializer.SerializeToElement(new { id = detector.Id, index = detector.Index }));
            store.SetState(4, detector.Id, JsonSerializer.SerializeToElement(new { state = 0, faultstate = 0, swico = 0, stateticks = 0UL }));
        }

        foreach (var output in config.Outputs)
        {
            store.SetMeta(6, output.Id, JsonSerializer.SerializeToElement(new { id = output.Id, index = output.Index }));
            store.SetState(6, output.Id, JsonSerializer.SerializeToElement(new { state = 0, faultstate = 0, stateticks = 0UL }));
        }

        foreach (var input in config.Inputs)
        {
            store.SetMeta(5, input.Id, JsonSerializer.SerializeToElement(new { id = input.Id, index = input.Index }));
            store.SetState(5, input.Id, JsonSerializer.SerializeToElement(new { state = 0, faultstate = 0, swico = 0, stateticks = 0UL }));
        }

        foreach (var variable in config.Variables)
        {
            store.SetMeta(8, variable.Id, JsonSerializer.SerializeToElement(new { id = variable.Id, index = variable.Index }));
            store.SetState(8, variable.Id, JsonSerializer.SerializeToElement(new { value = 0, lifetime = 0 }));
        }

        foreach (var sg in config.SignalGroups)
        {
            store.SetMeta(3, sg.Id, JsonSerializer.SerializeToElement(new { id = sg.Id, index = sg.Index }));
            store.SetState(3, sg.Id, JsonSerializer.SerializeToElement(new { state = 0, stateticks = 0UL, predictions = Array.Empty<object>(), dynLF = 0 }));
        }

        foreach (var intersection in config.Intersections)
        {
            store.SetMeta(2, intersection.Id, JsonSerializer.SerializeToElement(new { id = intersection.Id, index = intersection.Index }));
            store.SetState(2, intersection.Id, JsonSerializer.SerializeToElement(new { state = 0, stateticks = 0UL, tlcOverrule = 0 }));
        }

        store.SetMeta(1, config.FacilitiesId, JsonSerializer.SerializeToElement(new
        {
            intersections = config.Intersections.Select(x => x.Id).ToArray(),
            signalgroups = config.SignalGroups.Select(x => x.Id).ToArray(),
            detectors = config.Detectors.Select(x => x.Id).ToArray(),
            inputs = config.Inputs.Select(x => x.Id).ToArray(),
            outputs = config.Outputs.Select(x => x.Id).ToArray(),
            variables = config.Variables.Select(x => x.Id).ToArray(),
            spvehgenerator = config.SpVehGeneratorId
        }));

        store.SetMeta(0, "SESSION", JsonSerializer.SerializeToElement(new { }));
        store.SetMeta(7, config.SpVehGeneratorId, JsonSerializer.SerializeToElement(new { id = config.SpVehGeneratorId }));
    }

    public static Dictionary<Guid, string> BuildUpdateState(SimulatorStateStore store, SubscriptionManager subscriptions, IReadOnlyList<ClientConnection> clients, ulong ticks)
    {
        var messages = new Dictionary<Guid, string>();
        foreach (var client in clients)
        {
            var perType = subscriptions.GetSubscriptions(client.Id);
            if (perType is null)
            {
                continue;
            }

            var updates = new List<object>();
            foreach (var (type, ids) in perType)
            {
                var states = store.State.Where(kv => kv.Key.Type == type && ids.Contains(kv.Key.Id)).Select(kv => kv.Value).ToArray();
                if (states.Length == 0)
                {
                    continue;
                }

                updates.Add(new
                {
                    objects = new { type, ids = ids.ToArray() },
                    states
                });
            }

            if (updates.Count == 0)
            {
                continue;
            }

            var payload = new
            {
                jsonrpc = "2.0",
                method = "UpdateState",
                @params = new
                {
                    ticks,
                    update = updates
                }
            };

            messages[client.Id] = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        return messages;
    }
}
