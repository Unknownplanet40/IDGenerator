using IdGen.Core.Enums;
using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface IResetRequestRepository
{
    Task<ResetRequest?> GetByIdAsync(int id);
    Task<ResetRequest?> GetLatestByUserIdAsync(int userId);
    Task<ResetRequest?> GetPendingByUserIdAsync(int userId);
    Task<IReadOnlyList<ResetRequest>> GetAllAsync();
    Task<IReadOnlyList<ResetRequest>> GetPendingAsync();
    Task<int> CreateAsync(ResetRequest request);
    Task UpdateStatusAsync(int requestId, ResetStatus status, DateTime? resolvedAt = null);
}

public class ResetRequestRepository : IResetRequestRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        r.id, r.user_id, r.status, r.requested_at, r.resolved_at,
        u.student_no, u.first_name, u.last_name, u.middle_name";

    public ResetRequestRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ResetRequest?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM ResetRequests r LEFT JOIN Users u ON r.user_id = u.id WHERE r.id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapResetRequest(reader);
        }
        return null;
    }

    public async Task<ResetRequest?> GetLatestByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM ResetRequests r LEFT JOIN Users u ON r.user_id = u.id WHERE r.user_id = @userId ORDER BY r.requested_at DESC LIMIT 1;";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapResetRequest(reader);
        }
        return null;
    }

    public async Task<ResetRequest?> GetPendingByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM ResetRequests r LEFT JOIN Users u ON r.user_id = u.id WHERE r.user_id = @userId AND r.status = 'Pending' LIMIT 1;";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapResetRequest(reader);
        }
        return null;
    }

    public async Task<IReadOnlyList<ResetRequest>> GetAllAsync()
    {
        var list = new List<ResetRequest>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM ResetRequests r LEFT JOIN Users u ON r.user_id = u.id ORDER BY r.requested_at DESC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapResetRequest(reader));
        }
        return list;
    }

    public async Task<IReadOnlyList<ResetRequest>> GetPendingAsync()
    {
        var list = new List<ResetRequest>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM ResetRequests r LEFT JOIN Users u ON r.user_id = u.id WHERE r.status = 'Pending' ORDER BY r.requested_at ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapResetRequest(reader));
        }
        return list;
    }

    public async Task<int> CreateAsync(ResetRequest request)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO ResetRequests (user_id, status, requested_at, resolved_at)
            VALUES (@userId, @status, @requestedAt, @resolvedAt);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@userId", request.UserId);
        cmd.Parameters.AddWithValue("@status", request.Status.ToString());
        cmd.Parameters.AddWithValue("@requestedAt", request.RequestedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@resolvedAt", request.ResolvedAt.HasValue ? request.ResolvedAt.Value.ToString("o") : DBNull.Value);

        object? result = await cmd.ExecuteScalarAsync();
        request.Id = Convert.ToInt32(result);
        return request.Id;
    }

    public async Task UpdateStatusAsync(int requestId, ResetStatus status, DateTime? resolvedAt = null)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE ResetRequests SET status = @status, resolved_at = @resolvedAt WHERE id = @id;";
        cmd.Parameters.AddWithValue("@status", status.ToString());
        cmd.Parameters.AddWithValue("@resolvedAt", (resolvedAt ?? DateTime.UtcNow).ToString("o"));
        cmd.Parameters.AddWithValue("@id", requestId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static ResetRequest MapResetRequest(SqliteDataReader reader)
    {
        string? studentNo = null;
        string? studentName = null;

        if (reader.FieldCount > 5)
        {
            if (!reader.IsDBNull(5)) studentNo = reader.GetString(5);
            if (!reader.IsDBNull(6) && !reader.IsDBNull(7))
            {
                string fn = reader.GetString(6);
                string ln = reader.GetString(7);
                string mn = (!reader.IsDBNull(8)) ? reader.GetString(8) : "";
                studentName = string.IsNullOrWhiteSpace(mn) ? $"{fn} {ln}" : $"{fn} {mn} {ln}";
            }
        }

        return new ResetRequest
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            Status = Enum.Parse<ResetStatus>(reader.GetString(2)),
            RequestedAt = DateTime.Parse(reader.GetString(3)),
            ResolvedAt = reader.IsDBNull(4) ? null : DateTime.Parse(reader.GetString(4)),
            StudentNo = studentNo,
            StudentName = studentName
        };
    }
}
