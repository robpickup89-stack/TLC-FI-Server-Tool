using TLCFiTool.Models;

namespace TLCFiTool.Core;

public sealed class StateStore
{
    public List<Detector> Detectors { get; } = new();
    public List<Output> Outputs { get; } = new();
    public List<Intersection> Intersections { get; } = new();

    public void SeedDefaults(int detectorCount, int outputCount)
    {
        Detectors.Clear();
        Outputs.Clear();

        for (var i = 1; i <= detectorCount; i++)
        {
            Detectors.Add(new Detector { Index = i, Id = $"DET{i}" });
        }

        for (var i = 1; i <= outputCount; i++)
        {
            Outputs.Add(new Output { Index = i, Id = $"OUT{i}" });
        }
    }
}
