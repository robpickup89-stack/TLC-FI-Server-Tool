using System.Text.Json.Serialization;

namespace TLCFI.Models;

public sealed class SimulatorConfig
{
    public string FacilitiesId { get; set; } = "SIM_FAC_1";
    public string SessionIdTemplate { get; set; } = "SIM_SESSION_{0}";
    public List<SimulatorItem> Detectors { get; set; } = [];
    public List<SimulatorItem> Outputs { get; set; } = [];
    public List<SimulatorItem> Inputs { get; set; } = [];
    public List<SimulatorItem> Variables { get; set; } = [];
    public List<SimulatorItem> SignalGroups { get; set; } = [];
    public List<SimulatorItem> Intersections { get; set; } = [];
    public string SpVehGeneratorId { get; set; } = "SpecialVehicleEvents";
    public Dictionary<string, JsonElement> StateOverrides { get; set; } = [];
    public SimulationSettings Settings { get; set; } = new();
}

public sealed class SimulationSettings
{
    public int UpdateIntervalMs { get; set; } = 250;
    public ulong TickStart { get; set; } = 81870000;
}

public sealed class SimulatorItem
{
    public string Id { get; set; } = string.Empty;
    public int Index { get; set; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = [];
}
