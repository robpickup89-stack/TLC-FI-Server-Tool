namespace TLCFI.Models;

public sealed record SessionInfo(string SessionId, IReadOnlyList<string> FacilityIds, AppVersion Version, int Type);
