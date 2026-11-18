namespace IdGen.Core.Models;

public class TemplateRecord
{
    public int Id { get; set; }
    public string Orientation { get; set; } = "Portrait";
    public string FrontJson { get; set; } = "{}";
    public string BackJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
