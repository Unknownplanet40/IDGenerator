using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface ITemplateRepository
{
    Task<TemplateRecord?> GetActiveAsync();
    Task<int> SaveAsync(TemplateRecord template);
}

public class TemplateRepository : ITemplateRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public TemplateRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TemplateRecord?> GetActiveAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, orientation, front_json, back_json, updated_at FROM Templates ORDER BY id DESC LIMIT 1;";

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new TemplateRecord
            {
                Id = reader.GetInt32(0),
                Orientation = reader.GetString(1),
                FrontJson = reader.GetString(2),
                BackJson = reader.GetString(3),
                UpdatedAt = DateTime.Parse(reader.GetString(4))
            };
        }
        return null;
    }

    public async Task<int> SaveAsync(TemplateRecord template)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();

        var existing = await GetActiveAsync();
        if (existing == null)
        {
            cmd.CommandText = @"
                INSERT INTO Templates (orientation, front_json, back_json, updated_at)
                VALUES (@orientation, @frontJson, @backJson, @updatedAt);
                SELECT last_insert_rowid();";
        }
        else
        {
            cmd.CommandText = @"
                UPDATE Templates
                SET orientation = @orientation,
                    front_json = @frontJson,
                    back_json = @backJson,
                    updated_at = @updatedAt
                WHERE id = @id;
                SELECT @id;";
            cmd.Parameters.AddWithValue("@id", existing.Id);
            template.Id = existing.Id;
        }

        cmd.Parameters.AddWithValue("@orientation", template.Orientation);
        cmd.Parameters.AddWithValue("@frontJson", template.FrontJson);
        cmd.Parameters.AddWithValue("@backJson", template.BackJson);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));

        object? result = await cmd.ExecuteScalarAsync();
        template.Id = Convert.ToInt32(result);
        return template.Id;
    }
}
