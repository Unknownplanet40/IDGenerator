using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface ITemplateAssetRepository
{
    Task<int> SaveAssetAsync(TemplateAsset asset);
    Task<TemplateAsset?> GetByIdAsync(int id);
    Task<TemplateAsset?> GetActiveAssetByKindAsync(string kind);
    Task DeleteAsync(int id);
    Task DeleteByKindAsync(string kind);
}

public class TemplateAssetRepository : ITemplateAssetRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TemplateAssetRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> SaveAssetAsync(TemplateAsset asset)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();

        // If kind is SchoolLogo, ensure only one active row
        if (asset.Kind == "SchoolLogo")
        {
            await DeleteByKindAsync("SchoolLogo");
        }

        cmd.CommandText = @"
            INSERT INTO TemplateAssets (kind, mime_type, file_size, data, created_at)
            VALUES (@kind, @mimeType, @fileSize, @data, @createdAt);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@kind", asset.Kind);
        cmd.Parameters.AddWithValue("@mimeType", asset.MimeType);
        cmd.Parameters.AddWithValue("@fileSize", asset.FileSize);
        cmd.Parameters.AddWithValue("@data", asset.Data);
        cmd.Parameters.AddWithValue("@createdAt", asset.CreatedAt.ToString("O"));

        object? result = await cmd.ExecuteScalarAsync();
        asset.Id = Convert.ToInt32(result);
        return asset.Id;
    }

    public async Task<TemplateAsset?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, kind, mime_type, file_size, data, created_at FROM TemplateAssets WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAsset(reader);
        }
        return null;
    }

    public async Task<TemplateAsset?> GetActiveAssetByKindAsync(string kind)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, kind, mime_type, file_size, data, created_at FROM TemplateAssets WHERE kind = @kind ORDER BY id DESC LIMIT 1;";
        cmd.Parameters.AddWithValue("@kind", kind);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapAsset(reader);
        }
        return null;
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM TemplateAssets WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteByKindAsync(string kind)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM TemplateAssets WHERE kind = @kind;";
        cmd.Parameters.AddWithValue("@kind", kind);
        await cmd.ExecuteNonQueryAsync();
    }

    private static TemplateAsset MapAsset(SqliteDataReader reader)
    {
        long bytesCount = reader.GetBytes(4, 0, null, 0, 0);
        byte[] dataBytes = new byte[bytesCount];
        reader.GetBytes(4, 0, dataBytes, 0, (int)bytesCount);

        return new TemplateAsset
        {
            Id = reader.GetInt32(0),
            Kind = reader.GetString(1),
            MimeType = reader.GetString(2),
            FileSize = reader.GetInt32(3),
            Data = dataBytes,
            CreatedAt = DateTime.Parse(reader.GetString(5))
        };
    }
}
