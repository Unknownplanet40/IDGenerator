using System.Text.Json;
using System.Text.RegularExpressions;
using IdGen.Core.Models.Template;

namespace IdGen.Core.Validation;

public static class TemplateValidator
{
    private static readonly Regex HexColorRegex = new(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

    public static bool IsValidHexColor(string? color, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(color)) return allowEmpty;
        return HexColorRegex.IsMatch(color.Trim());
    }

    public static ValidationResult ValidateSide(TemplateSideDefinition? side)
    {
        var result = new ValidationResult();

        if (side == null)
        {
            result.AddError("side", "Template side definition cannot be null.");
            return result;
        }

        // Validate Background
        if (side.Background != null)
        {
            if (!IsValidHexColor(side.Background.Color, allowEmpty: false))
            {
                result.AddError("background.color", "Background color must be a valid hex color code (e.g. #FFFFFF).");
            }

            if (!TemplateConstants.BackgroundFitModes.Contains(side.Background.Fit, StringComparer.OrdinalIgnoreCase))
            {
                result.AddError("background.fit", $"Invalid background fit mode. Allowed: {string.Join(", ", TemplateConstants.BackgroundFitModes)}.");
            }
        }

        // Validate Elements count
        if (side.Elements == null)
        {
            result.AddError("elements", "Elements list cannot be null.");
            return result;
        }

        if (side.Elements.Count > TemplateConstants.MaxElementsPerSide)
        {
            result.AddError("elements", $"Cannot exceed {TemplateConstants.MaxElementsPerSide} elements per side. Found {side.Elements.Count}.");
        }

        for (int i = 0; i < side.Elements.Count; i++)
        {
            var el = side.Elements[i];
            string prefix = $"elements[{i}]";

            if (string.IsNullOrWhiteSpace(el.Type) || !TemplateConstants.ElementTypes.Contains(el.Type, StringComparer.OrdinalIgnoreCase))
            {
                result.AddError($"{prefix}.type", $"Unknown or unsupported element type '{el.Type}'.");
                continue;
            }

            // Geometry checks
            if (el.Width < 0 || double.IsNaN(el.Width))
                result.AddError($"{prefix}.width", "Width cannot be negative.");
            if (el.Height < 0 || double.IsNaN(el.Height))
                result.AddError($"{prefix}.height", "Height cannot be negative.");

            switch (el.Type)
            {
                case "Rectangle":
                    if (!IsValidHexColor(el.FillColor, allowEmpty: true))
                        result.AddError($"{prefix}.fill_color", "Rectangle fill color must be a valid hex code or empty.");
                    if (el.FillOpacity < 0 || el.FillOpacity > 100)
                        result.AddError($"{prefix}.fill_opacity", "Rectangle fill opacity must be between 0 and 100.");
                    if (!IsValidHexColor(el.BorderColor, allowEmpty: true))
                        result.AddError($"{prefix}.border_color", "Rectangle border color must be a valid hex code or empty.");
                    if (el.BorderWidth < 0 || el.BorderWidth > 8)
                        result.AddError($"{prefix}.border_width", "Rectangle border width must be between 0 and 8.");
                    if (el.CornerRadius < 0 || el.CornerRadius > 24)
                        result.AddError($"{prefix}.corner_radius", "Rectangle corner radius must be between 0 and 24.");
                    break;

                case "Line":
                    if (!IsValidHexColor(el.LineColor, allowEmpty: false))
                        result.AddError($"{prefix}.line_color", "Line color must be a valid hex code.");
                    if (el.Thickness < 1 || el.Thickness > 16)
                        result.AddError($"{prefix}.thickness", "Line thickness must be between 1 and 16.");
                    break;

                case "Barcode":
                    if (!IsValidHexColor(el.BarcodeColor, allowEmpty: false))
                        result.AddError($"{prefix}.barcode_color", "Barcode color must be a valid hex code.");
                    break;

                case "StaticText":
                    if (!string.IsNullOrEmpty(el.StaticText) && el.StaticText.Length > 200)
                        result.AddError($"{prefix}.static_text", "Static text cannot exceed 200 characters.");
                    ValidateTextElement(el, prefix, result);
                    break;

                case "DynamicText":
                    if (string.IsNullOrWhiteSpace(el.FieldKey) || !TemplateConstants.WhitelistedDynamicFields.Contains(el.FieldKey, StringComparer.OrdinalIgnoreCase))
                        result.AddError($"{prefix}.field_key", $"Field key '{el.FieldKey}' is not in the whitelist.");
                    ValidateTextElement(el, prefix, result);
                    break;

                case "Logo":
                case "Photo":
                case "Signature":
                    // Valid standard card widgets
                    break;
            }
        }

        return result;
    }

    private static void ValidateTextElement(TemplateElementDefinition el, string prefix, ValidationResult result)
    {
        if (!TemplateConstants.WhitelistedFonts.Contains(el.FontFamily, StringComparer.OrdinalIgnoreCase))
        {
            result.AddError($"{prefix}.font_family", $"Font family '{el.FontFamily}' is not in the whitelist.");
        }

        if (el.FontSize < 6 || el.FontSize > 72)
        {
            result.AddError($"{prefix}.font_size", "Font size must be between 6 and 72.");
        }

        if (!IsValidHexColor(el.TextColor, allowEmpty: false))
        {
            result.AddError($"{prefix}.text_color", "Text color must be a valid hex code.");
        }
    }

    public static TemplateSideDefinition ParseOrDefault(string? json, bool isFront = true)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return CreateDefaultSide(isFront);
        }

        try
        {
            var side = JsonSerializer.Deserialize<TemplateSideDefinition>(json);
            if (side != null && ValidateSide(side).IsValid)
            {
                return side;
            }
        }
        catch
        {
            // Parse failure
        }

        return CreateDefaultSide(isFront);
    }

    public static TemplateSideDefinition CreateDefaultSide(bool isFront)
    {
        var side = new TemplateSideDefinition
        {
            Background = new BackgroundDefinition
            {
                Color = "#F8FAFC",
                Fit = "Cover"
            }
        };

        if (isFront)
        {
            // Front elements
            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Rectangle",
                X = 0,
                Y = 0,
                Width = TemplateConstants.StandardPortraitWidth,
                Height = 140,
                FillColor = "#1E293B",
                FillOpacity = 100,
                BorderWidth = 0
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 20,
                Y = 30,
                Width = 598,
                Height = 35,
                FieldKey = "school_name",
                FontFamily = "Segoe UI",
                FontSize = 20,
                FontWeight = "Bold",
                TextColor = "#FFFFFF",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 20,
                Y = 75,
                Width = 598,
                Height = 25,
                FieldKey = "campus_name",
                FontFamily = "Segoe UI",
                FontSize = 14,
                FontWeight = "Normal",
                TextColor = "#94A3B8",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Photo",
                X = 209,
                Y = 170,
                Width = 220,
                Height = 220
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 30,
                Y = 415,
                Width = 578,
                Height = 40,
                FieldKey = "full_name",
                FontFamily = "Segoe UI",
                FontSize = 22,
                FontWeight = "Bold",
                TextColor = "#0F172A",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 30,
                Y = 460,
                Width = 578,
                Height = 28,
                FieldKey = "student_no",
                FontFamily = "Segoe UI",
                FontSize = 16,
                FontWeight = "SemiBold",
                TextColor = "#475569",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 30,
                Y = 495,
                Width = 578,
                Height = 26,
                FieldKey = "course",
                FontFamily = "Segoe UI",
                FontSize = 15,
                FontWeight = "Normal",
                TextColor = "#2563EB",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Barcode",
                X = 119,
                Y = 860,
                Width = 400,
                Height = 90,
                BarcodeColor = "#000000",
                ShowHumanReadable = true,
                IncludeQuietZone = true
            });
        }
        else
        {
            // Back elements
            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "StaticText",
                X = 40,
                Y = 40,
                Width = 558,
                Height = 80,
                StaticText = "This card is non-transferable and must be worn at all times while on university premises. In case of emergency or loss, notify authorities immediately.",
                FontFamily = "Segoe UI",
                FontSize = 12,
                TextColor = "#475569",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Line",
                X = 40,
                Y = 700,
                Width = 558,
                Height = 2,
                LineColor = "#CBD5E1",
                Thickness = 2
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Signature",
                X = 189,
                Y = 610,
                Width = 260,
                Height = 90
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "StaticText",
                X = 40,
                Y = 710,
                Width = 558,
                Height = 25,
                StaticText = "Cardholder's Signature",
                FontFamily = "Segoe UI",
                FontSize = 12,
                TextColor = "#64748B",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 40,
                Y = 870,
                Width = 558,
                Height = 30,
                FieldKey = "principal_name",
                FontFamily = "Segoe UI",
                FontSize = 16,
                FontWeight = "Bold",
                TextColor = "#0F172A",
                TextAlign = "Center"
            });

            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "DynamicText",
                X = 40,
                Y = 905,
                Width = 558,
                Height = 25,
                FieldKey = "principal_title",
                FontFamily = "Segoe UI",
                FontSize = 13,
                TextColor = "#64748B",
                TextAlign = "Center"
            });
        }

        return side;
    }
}
