using System.Text.Json.Serialization;

namespace IdGen.Core.Models.Template;

public class TemplateElementDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("type")]
    public string Type { get; set; } = "StaticText";

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; } = 100;

    [JsonPropertyName("height")]
    public double Height { get; set; } = 30;

    [JsonPropertyName("locked")]
    public bool Locked { get; set; }

    // Rectangle Properties
    [JsonPropertyName("fill_color")]
    public string? FillColor { get; set; }

    [JsonPropertyName("fill_opacity")]
    public double FillOpacity { get; set; } = 100.0;

    [JsonPropertyName("border_color")]
    public string? BorderColor { get; set; }

    [JsonPropertyName("border_width")]
    public double BorderWidth { get; set; }

    [JsonPropertyName("corner_radius")]
    public double CornerRadius { get; set; }

    // Line Properties
    [JsonPropertyName("line_color")]
    public string? LineColor { get; set; } = "#000000";

    [JsonPropertyName("thickness")]
    public double Thickness { get; set; } = 2.0;

    // Barcode Properties
    [JsonPropertyName("barcode_color")]
    public string? BarcodeColor { get; set; } = "#000000";

    [JsonPropertyName("show_human_readable")]
    public bool ShowHumanReadable { get; set; } = true;

    [JsonPropertyName("include_quiet_zone")]
    public bool IncludeQuietZone { get; set; } = true;

    // Text Properties
    [JsonPropertyName("static_text")]
    public string? StaticText { get; set; }

    [JsonPropertyName("field_key")]
    public string? FieldKey { get; set; }

    [JsonPropertyName("font_family")]
    public string FontFamily { get; set; } = "Segoe UI";

    [JsonPropertyName("font_size")]
    public double FontSize { get; set; } = 14.0;

    [JsonPropertyName("font_weight")]
    public string FontWeight { get; set; } = "Normal";

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }

    [JsonPropertyName("text_align")]
    public string TextAlign { get; set; } = "Left";

    [JsonPropertyName("text_color")]
    public string TextColor { get; set; } = "#000000";

    // Asset reference (e.g. custom logo asset id if not using default SchoolLogo)
    [JsonPropertyName("asset_id")]
    public int? AssetId { get; set; }

    public TemplateElementDefinition Clone()
    {
        return new TemplateElementDefinition
        {
            Id = Guid.NewGuid().ToString("N"),
            Type = Type,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Locked = Locked,
            FillColor = FillColor,
            FillOpacity = FillOpacity,
            BorderColor = BorderColor,
            BorderWidth = BorderWidth,
            CornerRadius = CornerRadius,
            LineColor = LineColor,
            Thickness = Thickness,
            BarcodeColor = BarcodeColor,
            ShowHumanReadable = ShowHumanReadable,
            IncludeQuietZone = IncludeQuietZone,
            StaticText = StaticText,
            FieldKey = FieldKey,
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontWeight = FontWeight,
            Italic = Italic,
            TextAlign = TextAlign,
            TextColor = TextColor,
            AssetId = AssetId
        };
    }
}
