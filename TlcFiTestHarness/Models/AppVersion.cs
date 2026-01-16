namespace TLCFI.Models;

public sealed record AppVersion(int Major, int Minor, int Revision)
{
    public override string ToString() => $"{Major}.{Minor}.{Revision}";
}
