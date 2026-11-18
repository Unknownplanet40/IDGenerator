using Microsoft.Data.Sqlite;

namespace IdGen.Data;

public interface ISqliteConnectionFactory
{
    string ConnectionString { get; }
    SqliteConnection CreateConnection();
}
