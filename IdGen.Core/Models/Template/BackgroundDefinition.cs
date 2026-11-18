using System.Text.Json.Serialization;

namespace IdGen.Core.Models.Template;

public class BackgroundDefinition
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "#FFFFFF";

    [JsonPropertyName("asset_id")]
    public int? AssetId { get; set; }

    [JsonPropertyName("fit")]
    public string Fit { get; set; } = "Cover";
}
