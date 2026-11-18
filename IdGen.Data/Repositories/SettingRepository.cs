using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface ISettingRepository
{
    Task<string> GetValueAsync(string key, string defaultValue = "");
    Task SetValueAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllAsync();
}

public class SettingRepository : ISettingRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SettingRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<string> GetValueAsync(string key, string defaultValue = "")
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM Settings WHERE key = @key LIMIT 1;";
        cmd.Parameters.AddWithValue("@key", key);

        object? result = await cmd.ExecuteScalarAsync();
        return result is string str ? str : defaultValue;
    }

    public async Task SetValueAsync(string key, string value)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Settings (key, value) VALUES (@key, @value)
            ON CONFLICT(key) DO UPDATE SET value = excluded.value;";
        cmd.Parameters.AddWithValue("@key", key);
        cmd.Parameters.AddWithValue("@value", value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT key, value FROM Settings;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            dict[reader.GetString(0)] = reader.GetString(1);
        }
        return dict;
    }
}
