using IdGen.Data;
using Microsoft.Data.Sqlite;
using Xunit;

namespace IdGen.Tests;

public class DatabaseMigrationTests
{
    [Fact]
    public void Migrate_SetsUserVersionToCurrent_AndIsIdempotent()
    {
        using var factory = new SqliteConnectionFactory($"Data Source=mig_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        using var conn = factory.CreateConnection();

        // First migration
        DatabaseMigrator.Migrate(conn);
        Assert.Equal(DatabaseMigrator.CurrentSchemaVersion, DatabaseMigrator.GetUserVersion(conn));

        // Second migration should be safe and idempotent
        DatabaseMigrator.Migrate(conn);
        Assert.Equal(DatabaseMigrator.CurrentSchemaVersion, DatabaseMigrator.GetUserVersion(conn));
    }

    [Fact]
    public void Migrate_CreatesAllExpectedTables()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        using var conn = factory.CreateConnection();

        var expectedTables = new[] { "Users", "Sections", "Settings", "IdSubmissions", "ResetRequests", "Notifications", "TemplateAssets", "Templates" };

        foreach (var table in expectedTables)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name = @name;";
            cmd.Parameters.AddWithValue("@name", table);
            long count = Convert.ToInt64(cmd.ExecuteScalar());
            Assert.True(count == 1, $"Table {table} was not created.");
        }
    }

    [Fact]
    public void Migrate_SeedsDefaultSettings()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        using var conn = factory.CreateConnection();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT key, value FROM Settings;";
        using var reader = cmd.ExecuteReader();

        var settings = new Dictionary<string, string>();
        while (reader.Read())
        {
            settings[reader.GetString(0)] = reader.GetString(1);
        }

        Assert.True(settings.ContainsKey("principal_name"));
        Assert.True(settings.ContainsKey("school_name"));
    }
}
