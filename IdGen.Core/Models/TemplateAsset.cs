namespace IdGen.Core.Models;

public class TemplateAsset
{
    public int Id { get; set; }
    public string Kind { get; set; } = string.Empty; // SchoolLogo, BackgroundFront, BackgroundBack
    public string MimeType { get; set; } = string.Empty;
    public int FileSize { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
