using System.Collections.ObjectModel;

namespace IdGen.Core.Constants;

public static class BloodTypes
{
    public static readonly ReadOnlyCollection<string> All = new(new[]
    {
        "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"
    });

    public static bool IsValid(string? bloodType)
    {
        if (string.IsNullOrWhiteSpace(bloodType)) return true; // Optional
        return All.Contains(bloodType.Trim().ToUpperInvariant());
    }
}
