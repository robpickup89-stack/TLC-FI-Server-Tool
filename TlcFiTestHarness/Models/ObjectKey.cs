namespace TLCFI.Models;

public readonly record struct ObjectKey(int Type, string Id)
{
    public override string ToString() => $"Type={Type} Id={Id}";
}
