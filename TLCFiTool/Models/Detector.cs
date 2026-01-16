namespace TLCFiTool.Models;

public sealed class Detector
{
    public int Index { get; set; }
    public string Id { get; set; } = string.Empty;
    public bool GeneratesEvents { get; set; }
    public int State { get; set; }
    public int FaultState { get; set; }
    public int Swico { get; set; }
    public long StateTicks { get; set; }
    public string Notes { get; set; } = string.Empty;
}
