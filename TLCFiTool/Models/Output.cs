namespace TLCFiTool.Models;

public sealed class Output
{
    public int Index { get; set; }
    public string Id { get; set; } = string.Empty;
    public int? State { get; set; }
    public int FaultState { get; set; }
    public int? ReqState { get; set; }
    public long StateTicks { get; set; }
    public bool IsExclusive { get; set; }
    public string Notes { get; set; } = string.Empty;
}
