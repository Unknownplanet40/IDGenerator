using System.Text.Json;
using IdGen.Core.Models;
using IdGen.Core.Models.Template;
using IdGen.Core.Services;
using IdGen.Core.Validation;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _templateRepository;

    public TemplateService(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<TemplateRecord> GetTemplateAsync()
    {
        var record = await _templateRepository.GetActiveAsync();
        if (record == null)
        {
            var frontRef = GetReferenceFront();
            var backRef = GetReferenceBack();

            record = new TemplateRecord
            {
                Orientation = "Portrait",
                FrontJson = JsonSerializer.Serialize(frontRef),
                BackJson = JsonSerializer.Serialize(backRef),
                UpdatedAt = DateTime.UtcNow
            };

            await _templateRepository.SaveAsync(record);
        }

        return record;
    }

    public async Task<TemplateSideDefinition> GetFrontSideAsync()
    {
        var record = await GetTemplateAsync();
        return TemplateValidator.ParseOrDefault(record.FrontJson, isFront: true);
    }

    public async Task<TemplateSideDefinition> GetBackSideAsync()
    {
        var record = await GetTemplateAsync();
        return TemplateValidator.ParseOrDefault(record.BackJson, isFront: false);
    }

    public async Task SaveTemplateAsync(string orientation, TemplateSideDefinition front, TemplateSideDefinition back)
    {
        var frontValidation = TemplateValidator.ValidateSide(front);
        if (!frontValidation.IsValid)
        {
            var firstErr = frontValidation.Errors.Values.FirstOrDefault() ?? "Front template validation failed.";
            throw new ArgumentException(firstErr, nameof(front));
        }

        var backValidation = TemplateValidator.ValidateSide(back);
        if (!backValidation.IsValid)
        {
            var firstErr = backValidation.Errors.Values.FirstOrDefault() ?? "Back template validation failed.";
            throw new ArgumentException(firstErr, nameof(back));
        }

        var record = new TemplateRecord
        {
            Orientation = orientation,
            FrontJson = JsonSerializer.Serialize(front),
            BackJson = JsonSerializer.Serialize(back),
            UpdatedAt = DateTime.UtcNow
        };

        await _templateRepository.SaveAsync(record);
    }

    public TemplateSideDefinition GetReferenceFront()
    {
        return TemplateDefaults.CreateDefaultFront();
    }

    public TemplateSideDefinition GetReferenceBack()
    {
        return TemplateDefaults.CreateDefaultBack();
    }
}
