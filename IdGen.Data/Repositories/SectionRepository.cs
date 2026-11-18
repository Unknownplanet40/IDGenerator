using IdGen.Core.Exceptions;
using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface ISectionRepository
{
    Task<IReadOnlyList<Section>> GetAllAsync();
    Task<IReadOnlyList<Section>> SearchAsync(string? query);
    Task<Section?> GetByIdAsync(int id);
    Task<Section?> GetByYearAndSectionAsync(string yearLevel, string sectionName);
    Task<Section?> GetByCompositeKeyAsync(string? course, string yearLevel, string sectionName, int? excludeId = null);
    Task<int> CreateAsync(Section section);
    Task UpdateAsync(Section section);
    Task DeleteAsync(int id);
    Task<bool> IsSectionInUseAsync(int sectionId);
}

public class SectionRepository : ISectionRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SectionRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Section>> GetAllAsync()
    {
        var sections = new List<Section>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, course, year_level, section_name, adviser FROM Sections ORDER BY COALESCE(course, ''), year_level, section_name;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sections.Add(MapSection(reader));
        }
        return sections;
    }

    public async Task<IReadOnlyList<Section>> SearchAsync(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        string pattern = $"%{query.Trim()}%";
        var sections = new List<Section>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, course, year_level, section_name, adviser 
            FROM Sections 
            WHERE (course LIKE @q OR year_level LIKE @q OR section_name LIKE @q OR adviser LIKE @q)
            ORDER BY COALESCE(course, ''), year_level, section_name;";
        cmd.Parameters.AddWithValue("@q", pattern);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sections.Add(MapSection(reader));
        }
        return sections;
    }

    public async Task<Section?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, course, year_level, section_name, adviser FROM Sections WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapSection(reader);
        }
        return null;
    }

    public async Task<Section?> GetByYearAndSectionAsync(string yearLevel, string sectionName)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, course, year_level, section_name, adviser FROM Sections WHERE year_level = @yearLevel AND section_name = @sectionName LIMIT 1;";
        cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
        cmd.Parameters.AddWithValue("@sectionName", sectionName);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapSection(reader);
        }
        return null;
    }

    public async Task<Section?> GetByCompositeKeyAsync(string? course, string yearLevel, string sectionName, int? excludeId = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();

        string sql = "SELECT id, course, year_level, section_name, adviser FROM Sections WHERE COALESCE(course, '') = @course AND year_level = @yearLevel AND section_name = @sectionName";
        if (excludeId.HasValue)
        {
            sql += " AND id != @excludeId";
            cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
        }
        sql += " LIMIT 1;";

        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@course", course ?? string.Empty);
        cmd.Parameters.AddWithValue("@yearLevel", yearLevel);
        cmd.Parameters.AddWithValue("@sectionName", sectionName);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapSection(reader);
        }
        return null;
    }

    public async Task<int> CreateAsync(Section section)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Sections (course, year_level, section_name, adviser)
            VALUES (@course, @yearLevel, @sectionName, @adviser);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@course", (object?)section.Course ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@yearLevel", section.YearLevel);
        cmd.Parameters.AddWithValue("@sectionName", section.SectionName);
        cmd.Parameters.AddWithValue("@adviser", section.Adviser);

        object? result = await cmd.ExecuteScalarAsync();
        section.Id = Convert.ToInt32(result);
        return section.Id;
    }

    public async Task UpdateAsync(Section section)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Sections
            SET course = @course,
                year_level = @yearLevel,
                section_name = @sectionName,
                adviser = @adviser
            WHERE id = @id;";

        cmd.Parameters.AddWithValue("@id", section.Id);
        cmd.Parameters.AddWithValue("@course", (object?)section.Course ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@yearLevel", section.YearLevel);
        cmd.Parameters.AddWithValue("@sectionName", section.SectionName);
        cmd.Parameters.AddWithValue("@adviser", section.Adviser);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> IsSectionInUseAsync(int sectionId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM IdSubmissions WHERE section_id = @sectionId;";
        cmd.Parameters.AddWithValue("@sectionId", sectionId);

        object? result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(result ?? 0) > 0;
    }

    public async Task DeleteAsync(int id)
    {
        if (await IsSectionInUseAsync(id))
        {
            throw new SectionInUseException(id);
        }

        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Sections WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Section MapSection(SqliteDataReader reader)
    {
        return new Section
        {
            Id = reader.GetInt32(0),
            Course = reader.IsDBNull(1) ? null : reader.GetString(1),
            YearLevel = reader.GetString(2),
            SectionName = reader.GetString(3),
            Adviser = reader.GetString(4)
        };
    }
}
