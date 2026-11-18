using IdGen.Core.Models.Template;
using IdGen.Data;
using IdGen.Data.Repositories;
using IdGen.Data.Services;
using Xunit;

namespace IdGen.Tests;

public class TemplateServiceTests
{
    [Fact]
    public async Task GetTemplateAsync_SeedsDefaultReferenceLayout_WhenNoneExists()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new TemplateRepository(factory);
        var service = new TemplateService(repo);

        var template = await service.GetTemplateAsync();
        Assert.NotNull(template);
        Assert.Equal("Portrait", template.Orientation);

        var front = await service.GetFrontSideAsync();
        Assert.NotNull(front);
        Assert.Contains(front.Elements, e => e.Type == "Barcode");
        Assert.Contains(front.Elements, e => e.Type == "Logo");
        Assert.Contains(front.Elements, e => e.Type == "Photo");
        Assert.Contains(front.Elements, e => e.Type == "Rectangle");

        var back = await service.GetBackSideAsync();
        Assert.NotNull(back);
        Assert.Contains(back.Elements, e => e.Type == "Signature");
        Assert.Contains(back.Elements, e => e.Type == "StaticText" && e.StaticText!.Contains("EMERGENCY"));
    }

    [Fact]
    public async Task SaveTemplateAsync_ValidatesAndUpdatesTemplate()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new TemplateRepository(factory);
        var service = new TemplateService(repo);

        var front = await service.GetFrontSideAsync();
        var back = await service.GetBackSideAsync();

        // Add a valid rectangle element
        front.Elements.Add(new TemplateElementDefinition
        {
            Type = "Rectangle",
            X = 10,
            Y = 10,
            Width = 100,
            Height = 50,
            FillColor = "#FF0000",
            FillOpacity = 80,
            CornerRadius = 12
        });

        await service.SaveTemplateAsync("Portrait", front, back);

        var updatedFront = await service.GetFrontSideAsync();
        Assert.Equal(front.Elements.Count, updatedFront.Elements.Count);
        Assert.Contains(updatedFront.Elements, e => e.FillColor == "#FF0000");
    }
}
