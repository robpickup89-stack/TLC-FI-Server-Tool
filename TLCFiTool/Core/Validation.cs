using TLCFiTool.Models;

namespace TLCFiTool.Core;

public static class Validation
{
    public static bool IsValidIndex(int index) => index is >= 1 and <= 255;

    public static bool HasUniqueDetectorIds(IEnumerable<Detector> detectors)
        => detectors.Select(d => d.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() == detectors.Count();

    public static bool HasUniqueOutputIds(IEnumerable<Output> outputs)
        => outputs.Select(o => o.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() == outputs.Count();

    public static bool HasUniqueDetectorIndices(IEnumerable<Detector> detectors)
        => detectors.Select(d => d.Index).Distinct().Count() == detectors.Count();

    public static bool HasUniqueOutputIndices(IEnumerable<Output> outputs)
        => outputs.Select(o => o.Index).Distinct().Count() == outputs.Count();
}
