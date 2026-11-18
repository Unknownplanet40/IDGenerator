using Microsoft.Data.Sqlite;

namespace IdGen.Data;

public class SqliteConnectionFactory : ISqliteConnectionFactory, IDisposable
{
    private readonly SqliteConnection? _masterConnection;

    public string ConnectionString { get; }

    public SqliteConnectionFactory(string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDir = Path.Combine(localAppData, "StudentIdGenerator");
            Directory.CreateDirectory(appDir);
            string dbPath = Path.Combine(appDir, "app.db");

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                ForeignKeys = true
            };
            ConnectionString = builder.ConnectionString;
        }
        else
        {
            var builder = new SqliteConnectionStringBuilder(connectionString)
            {
                ForeignKeys = true
            };
            ConnectionString = builder.ConnectionString;

            bool isMemory = builder.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase) ||
                            builder.Mode == SqliteOpenMode.Memory ||
                            ConnectionString.Contains("Mode=Memory", StringComparison.OrdinalIgnoreCase);

            if (isMemory)
            {
                _masterConnection = new SqliteConnection(ConnectionString);
                _masterConnection.Open();
                using var cmd = _masterConnection.CreateCommand();
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        return connection;
    }

    public void Dispose()
    {
        _masterConnection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
