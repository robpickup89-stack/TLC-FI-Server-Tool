namespace TLCFiTool.Models;

public sealed class Intersection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> DetectorIds { get; set; } = new();
    public List<string> OutputIds { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
