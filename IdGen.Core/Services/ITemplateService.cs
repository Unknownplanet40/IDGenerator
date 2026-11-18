using IdGen.Core.Models;
using IdGen.Core.Models.Template;

namespace IdGen.Core.Services;

public interface ITemplateService
{
    Task<TemplateRecord> GetTemplateAsync();
    Task<TemplateSideDefinition> GetFrontSideAsync();
    Task<TemplateSideDefinition> GetBackSideAsync();
    Task SaveTemplateAsync(string orientation, TemplateSideDefinition front, TemplateSideDefinition back);
    TemplateSideDefinition GetReferenceFront();
    TemplateSideDefinition GetReferenceBack();
}
