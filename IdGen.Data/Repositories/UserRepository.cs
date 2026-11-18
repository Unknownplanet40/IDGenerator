using System.Data;
using IdGen.Core.Enums;
using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByStudentNoAsync(string studentNo);
    Task<bool> HasAnyAdminAsync();
    Task<IReadOnlyList<User>> GetPendingStudentsAsync();
    Task<IReadOnlyList<User>> GetAllStudentsAsync();
    Task<IReadOnlyList<User>> GetAllUsersAsync();
    Task<int> CreateAsync(User user);
    Task UpdateStatusAsync(int userId, RegistrationStatus status);
    Task UpdatePasswordAsync(int userId, string passwordHash, string salt, int iterations);
    Task DeleteAsync(int userId);
}

public class UserRepository : IUserRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public UserRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users WHERE id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users WHERE username = @username COLLATE NOCASE LIMIT 1;";
        cmd.Parameters.AddWithValue("@username", username);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<User?> GetByStudentNoAsync(string studentNo)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users WHERE student_no = @studentNo LIMIT 1;";
        cmd.Parameters.AddWithValue("@studentNo", studentNo);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<bool> HasAnyAdminAsync()
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE role = 'Admin';";
        object? result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt64(result ?? 0) > 0;
    }

    public async Task<IReadOnlyList<User>> GetPendingStudentsAsync()
    {
        var users = new List<User>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users WHERE role = 'Student' AND status = 'Pending' ORDER BY created_at ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public async Task<IReadOnlyList<User>> GetAllStudentsAsync()
    {
        var users = new List<User>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users WHERE role = 'Student' ORDER BY last_name, first_name;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public async Task<IReadOnlyList<User>> GetAllUsersAsync()
    {
        var users = new List<User>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at FROM Users ORDER BY role ASC, last_name ASC, first_name ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public async Task<int> CreateAsync(User user)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (role, username, password_hash, salt, iterations, student_no, last_name, first_name, middle_name, email, phone, status, created_at)
            VALUES (@role, @username, @passwordHash, @salt, @iterations, @studentNo, @lastName, @firstName, @middleName, @email, @phone, @status, @createdAt);
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@role", user.Role.ToString());
        cmd.Parameters.AddWithValue("@username", user.Username);
        cmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@salt", user.Salt);
        cmd.Parameters.AddWithValue("@iterations", user.Iterations);
        cmd.Parameters.AddWithValue("@studentNo", (object?)user.StudentNo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lastName", user.LastName);
        cmd.Parameters.AddWithValue("@firstName", user.FirstName);
        cmd.Parameters.AddWithValue("@middleName", (object?)user.MiddleName ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@email", user.Email);
        cmd.Parameters.AddWithValue("@phone", user.Phone);
        cmd.Parameters.AddWithValue("@status", user.Status.ToString());
        cmd.Parameters.AddWithValue("@createdAt", user.CreatedAt.ToString("o"));

        object? result = await cmd.ExecuteScalarAsync();
        user.Id = Convert.ToInt32(result);
        return user.Id;
    }

    public async Task UpdateStatusAsync(int userId, RegistrationStatus status)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET status = @status WHERE id = @id;";
        cmd.Parameters.AddWithValue("@status", status.ToString());
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, string salt, int iterations)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET password_hash = @passwordHash, salt = @salt, iterations = @iterations WHERE id = @id;";
        cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
        cmd.Parameters.AddWithValue("@salt", salt);
        cmd.Parameters.AddWithValue("@iterations", iterations);
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static User MapUser(SqliteDataReader reader)
    {
        return new User
        {
            Id = reader.GetInt32(0),
            Role = Enum.Parse<UserRole>(reader.GetString(1)),
            Username = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            Salt = reader.GetString(4),
            Iterations = reader.GetInt32(5),
            StudentNo = reader.IsDBNull(6) ? null : reader.GetString(6),
            LastName = reader.GetString(7),
            FirstName = reader.GetString(8),
            MiddleName = reader.IsDBNull(9) ? null : reader.GetString(9),
            Email = reader.GetString(10),
            Phone = reader.GetString(11),
            Status = Enum.Parse<RegistrationStatus>(reader.GetString(12)),
            CreatedAt = DateTime.Parse(reader.GetString(13))
        };
    }
}
