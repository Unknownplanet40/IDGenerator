using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id);
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(int userId);
    Task<int> CreateAsync(Notification notification);
    Task MarkAsReadAsync(int notificationId);
}

public class NotificationRepository : INotificationRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public NotificationRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, user_id, message, created_at, is_read FROM Notifications WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapNotification(reader);
        }
        return null;
    }

    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(int userId)
    {
        var list = new List<Notification>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, user_id, message, created_at, is_read FROM Notifications WHERE user_id = @userId ORDER BY created_at DESC;";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapNotification(reader));
        }
        return list;
    }

    public async Task<int> CreateAsync(Notification notification)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Notifications (user_id, message, created_at, is_read)
            VALUES (@userId, @message, @createdAt, @isRead);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@userId", notification.UserId);
        cmd.Parameters.AddWithValue("@message", notification.Message);
        cmd.Parameters.AddWithValue("@createdAt", notification.CreatedAt.ToString("o"));
        cmd.Parameters.AddWithValue("@isRead", notification.IsRead ? 1 : 0);

        object? result = await cmd.ExecuteScalarAsync();
        notification.Id = Convert.ToInt32(result);
        return notification.Id;
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Notifications SET is_read = 1 WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", notificationId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Notification MapNotification(SqliteDataReader reader)
    {
        return new Notification
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            Message = reader.GetString(2),
            CreatedAt = DateTime.Parse(reader.GetString(3)),
            IsRead = reader.GetInt32(4) == 1
        };
    }
}
