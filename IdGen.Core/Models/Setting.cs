namespace IdGen.Core.Models;

public class Setting
{
    public const string PrincipalNameKey = "principal_name";
    public const string SchoolNameKey = "school_name";

    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
