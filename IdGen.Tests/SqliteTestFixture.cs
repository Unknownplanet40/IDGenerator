using IdGen.Data;

namespace IdGen.Tests;

public static class SqliteTestFixture
{
    public static SqliteConnectionFactory CreateInMemoryFactory()
    {
        string dbName = $"mem_{Guid.NewGuid():N}";
        var factory = new SqliteConnectionFactory($"Data Source={dbName};Mode=Memory;Cache=Shared");
        using var conn = factory.CreateConnection();
        DatabaseMigrator.Migrate(conn);
        return factory;
    }
}
