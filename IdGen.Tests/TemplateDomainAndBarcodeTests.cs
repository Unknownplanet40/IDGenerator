using System.Text.Json;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using Xunit;

namespace IdGen.Tests;

public class TemplateDomainAndBarcodeTests
{
    [Fact]
    public void IsValidHexColor_ValidatesCorrectly()
    {
        Assert.True(TemplateValidator.IsValidHexColor("#FFF"));
        Assert.True(TemplateValidator.IsValidHexColor("#FFFFFF"));
        Assert.True(TemplateValidator.IsValidHexColor("#1E293BAA"));
        Assert.True(TemplateValidator.IsValidHexColor("#123456"));

        Assert.False(TemplateValidator.IsValidHexColor("FFFFFF")); // Missing #
        Assert.False(TemplateValidator.IsValidHexColor("#GGGGGG")); // Invalid hex
        Assert.False(TemplateValidator.IsValidHexColor("#12345"));   // Invalid length
        Assert.False(TemplateValidator.IsValidHexColor(""));
        Assert.True(TemplateValidator.IsValidHexColor("", allowEmpty: true));
    }

    [Fact]
    public void ValidateSide_RejectsUnknownElementType()
    {
        var side = new TemplateSideDefinition
        {
            Elements = new List<TemplateElementDefinition>
            {
                new() { Type = "CircularPieChart", Width = 50, Height = 50 }
            }
        };

        var result = TemplateValidator.ValidateSide(side);
        Assert.False(result.IsValid);
        Assert.True(result.HasError("elements[0].type"));
    }

    [Fact]
    public void ValidateSide_RejectsUnwhitelistedFont()
    {
        var side = new TemplateSideDefinition
        {
            Elements = new List<TemplateElementDefinition>
            {
                new() { Type = "StaticText", FontFamily = "Comic Sans MS", StaticText = "Hello" }
            }
        };

        var result = TemplateValidator.ValidateSide(side);
        Assert.False(result.IsValid);
        Assert.True(result.HasError("elements[0].font_family"));
    }

    [Fact]
    public void ValidateSide_RejectsUnwhitelistedFieldKey()
    {
        var side = new TemplateSideDefinition
        {
            Elements = new List<TemplateElementDefinition>
            {
                new() { Type = "DynamicText", FieldKey = "unauthorized_secret_key" }
            }
        };

        var result = TemplateValidator.ValidateSide(side);
        Assert.False(result.IsValid);
        Assert.True(result.HasError("elements[0].field_key"));
    }

    [Fact]
    public void ValidateSide_EnforcesMax80Elements()
    {
        var side = new TemplateSideDefinition();
        for (int i = 0; i < 85; i++)
        {
            side.Elements.Add(new TemplateElementDefinition
            {
                Type = "Line",
                Thickness = 2,
                LineColor = "#000000"
            });
        }

        var result = TemplateValidator.ValidateSide(side);
        Assert.False(result.IsValid);
        Assert.True(result.HasError("elements"));
    }

    [Fact]
    public void ParseOrDefault_ReturnsDefaultOnInvalidJson()
    {
        var side = TemplateValidator.ParseOrDefault("not-valid-json", isFront: true);
        Assert.NotNull(side);
        Assert.NotEmpty(side.Elements);
        Assert.Contains(side.Elements, e => e.Type == "Barcode");
    }

    [Fact]
    public void Code128_EncodesEvenDigits_UsingCodeC()
    {
        // "123456" in Code C:
        // Start C = 105
        // '12' = 12
        // '34' = 34
        // '56' = 56
        // Sum = 105 + 1*12 + 2*34 + 3*56 = 105 + 12 + 68 + 168 = 353
        // Checksum = 353 % 103 = 44
        // Stop = 106
        var res = Code128Encoder.Encode("123456");

        Assert.Equal(105, res.SymbolCodes[0]);
        Assert.Equal(12, res.SymbolCodes[1]);
        Assert.Equal(34, res.SymbolCodes[2]);
        Assert.Equal(56, res.SymbolCodes[3]);
        Assert.Equal(44, res.Checksum);
        Assert.Equal(44, res.SymbolCodes[4]);
        Assert.Equal(106, res.SymbolCodes[5]);
    }

    [Fact]
    public void Code128_EncodesOddStudentNumber_Correctly()
    {
        // Student ID: "202110055" (9 digits)
        var res = Code128Encoder.Encode("202110055");
        Assert.NotNull(res);
        Assert.True(res.Checksum >= 0 && res.Checksum < 103);
        Assert.Equal(106, res.SymbolCodes.Last());
        Assert.NotEmpty(res.Bars);
        Assert.True(res.TotalModules > 50);
    }

    [Fact]
    public void Code128_RejectsNonAscii()
    {
        Assert.Throws<ArgumentException>(() => Code128Encoder.Encode("Hello \u00E9"));
        Assert.Throws<ArgumentException>(() => Code128Encoder.Encode(""));
    }

    [Fact]
    public void ReferenceLayout_CanBeSerialized_Validated_AndDeserialized()
    {
        var front = TemplateValidator.CreateDefaultSide(isFront: true);
        var back = TemplateValidator.CreateDefaultSide(isFront: false);

        string frontJson = JsonSerializer.Serialize(front);
        string backJson = JsonSerializer.Serialize(back);

        var parsedFront = TemplateValidator.ParseOrDefault(frontJson, isFront: true);
        var parsedBack = TemplateValidator.ParseOrDefault(backJson, isFront: false);

        Assert.Equal(front.Elements.Count, parsedFront.Elements.Count);
        Assert.Equal(back.Elements.Count, parsedBack.Elements.Count);
    }
}
