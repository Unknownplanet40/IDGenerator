using System.Text.Json.Serialization;

namespace IdGen.Core.Models.Template;

public class TemplateSideDefinition
{
    [JsonPropertyName("background")]
    public BackgroundDefinition Background { get; set; } = new();

    [JsonPropertyName("elements")]
    public List<TemplateElementDefinition> Elements { get; set; } = new();

    public TemplateSideDefinition Clone()
    {
        return new TemplateSideDefinition
        {
            Background = new BackgroundDefinition
            {
                Color = Background.Color,
                AssetId = Background.AssetId,
                Fit = Background.Fit
            },
            Elements = Elements.Select(e => e.Clone()).ToList()
        };
    }
}
