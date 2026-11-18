using System.IO;
using System.Text.Json;
using IdGen.Data.Services;
using Microsoft.Data.Sqlite;

namespace IdGen.Data;

public static class DatabaseMigrator
{
    public const int CurrentSchemaVersion = 4;

    public static void Migrate(SqliteConnection connection)
    {
        int currentVersion = GetUserVersion(connection);

        if (currentVersion < 1)
        {
            ApplyVersion1(connection);
            SetUserVersion(connection, 1);
        }

        if (currentVersion < 2)
        {
            ApplyVersion2(connection);
            SetUserVersion(connection, 2);
        }

        if (currentVersion < 3)
        {
            ApplyVersion3(connection);
            SetUserVersion(connection, 3);
        }

        if (currentVersion < 4)
        {
            ApplyVersion4(connection);
            SetUserVersion(connection, 4);
        }
    }

    public static int GetUserVersion(SqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA user_version;";
        object? result = cmd.ExecuteScalar();
        return Convert.ToInt32(result);
    }

    public static void SetUserVersion(SqliteConnection connection, int version)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"PRAGMA user_version = {version};";
        cmd.ExecuteNonQuery();
    }

    private static void ApplyVersion1(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        const string schemaSql = @"
            CREATE TABLE IF NOT EXISTS Users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                role TEXT NOT NULL CHECK(role IN ('Admin', 'Student')),
                username TEXT NOT NULL COLLATE NOCASE,
                password_hash TEXT NOT NULL,
                salt TEXT NOT NULL,
                iterations INTEGER NOT NULL,
                student_no TEXT NULL,
                last_name TEXT NOT NULL,
                first_name TEXT NOT NULL,
                middle_name TEXT NULL,
                email TEXT NOT NULL,
                phone TEXT NOT NULL,
                status TEXT NOT NULL CHECK(status IN ('Pending', 'Approved', 'Declined')),
                created_at TEXT NOT NULL,
                CONSTRAINT UQ_Users_Username UNIQUE (username COLLATE NOCASE),
                CONSTRAINT UQ_Users_StudentNo UNIQUE (student_no)
            );

            CREATE TABLE IF NOT EXISTS Sections (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                course TEXT NULL,
                year_level TEXT NOT NULL,
                section_name TEXT NOT NULL,
                adviser TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS IdSubmissions (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                gender TEXT NOT NULL,
                birth_date TEXT NOT NULL,
                address TEXT NOT NULL,
                guardian_name TEXT NOT NULL,
                guardian_phone TEXT NOT NULL,
                section_id INTEGER NOT NULL,
                adviser_snapshot TEXT NOT NULL,
                principal_snapshot TEXT NOT NULL,
                school_snapshot TEXT NOT NULL,
                signature_png BLOB NOT NULL,
                photo_png BLOB NULL,
                status TEXT NOT NULL CHECK(status IN ('Pending', 'Approved', 'Disapproved')),
                submitted_at TEXT NOT NULL,
                CONSTRAINT UQ_IdSubmissions_UserId UNIQUE (user_id),
                FOREIGN KEY(user_id) REFERENCES Users(id) ON DELETE CASCADE,
                FOREIGN KEY(section_id) REFERENCES Sections(id) ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS ResetRequests (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                status TEXT NOT NULL CHECK(status IN ('Pending', 'Approved', 'Declined')),
                requested_at TEXT NOT NULL,
                resolved_at TEXT NULL,
                FOREIGN KEY(user_id) REFERENCES Users(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Notifications (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                user_id INTEGER NOT NULL,
                message TEXT NOT NULL,
                created_at TEXT NOT NULL,
                is_read INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY(user_id) REFERENCES Users(id) ON DELETE CASCADE
            );

            -- Default settings rows if not present
            INSERT OR IGNORE INTO Settings (key, value) VALUES ('principal_name', '');
            INSERT OR IGNORE INTO Settings (key, value) VALUES ('school_name', '');
        ";

        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = schemaSql;
        cmd.ExecuteNonQuery();

        transaction.Commit();
    }

    private static void ApplyVersion2(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        // Helper to safely add column if not exists
        void EnsureColumnExists(string tableName, string columnName, string columnDefinition)
        {
            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.Transaction = transaction;
            pragmaCmd.CommandText = $"PRAGMA table_info({tableName});";

            bool columnExists = false;
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string existingCol = reader.GetString(1);
                    if (existingCol.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                using var alterCmd = connection.CreateCommand();
                alterCmd.Transaction = transaction;
                alterCmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};";
                alterCmd.ExecuteNonQuery();
            }
        }

        // 1. Add course column to Sections
        EnsureColumnExists("Sections", "course", "TEXT NULL");

        // 2. Add columns to IdSubmissions
        EnsureColumnExists("IdSubmissions", "blood_type", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "course_snapshot", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "campus_snapshot", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "principal_title_snapshot", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "semester_snapshot", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "school_year_snapshot", "TEXT NULL");
        EnsureColumnExists("IdSubmissions", "photo_png", "BLOB NULL");

        // 3. Ensure composite unique index on Sections and unique on IdSubmissions
        using (var idxCmd = connection.CreateCommand())
        {
            idxCmd.Transaction = transaction;
            idxCmd.CommandText = @"
                CREATE UNIQUE INDEX IF NOT EXISTS idx_sections_composite ON Sections(COALESCE(course, ''), year_level, section_name);
                CREATE UNIQUE INDEX IF NOT EXISTS idx_idsubmissions_user_id ON IdSubmissions(user_id);
            ";
            idxCmd.ExecuteNonQuery();
        }

        // 4. Create TemplateAssets and Templates tables, seed new Settings keys
        const string tablesSql = @"
            CREATE TABLE IF NOT EXISTS TemplateAssets (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                kind TEXT NOT NULL,
                mime_type TEXT NOT NULL,
                file_size INTEGER NOT NULL,
                data BLOB NOT NULL,
                created_at TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Templates (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                orientation TEXT NOT NULL,
                front_json TEXT NOT NULL,
                back_json TEXT NOT NULL,
                updated_at TEXT NOT NULL
            );

            INSERT OR IGNORE INTO Settings (key, value) VALUES ('campus_name', '');
            INSERT OR IGNORE INTO Settings (key, value) VALUES ('principal_title', '');
            INSERT OR IGNORE INTO Settings (key, value) VALUES ('semester', 'First');
            INSERT OR IGNORE INTO Settings (key, value) VALUES ('school_year', '');
        ";

        using (var tablesCmd = connection.CreateCommand())
        {
            tablesCmd.Transaction = transaction;
            tablesCmd.CommandText = tablesSql;
            tablesCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static void ApplyVersion3(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        using (var pragmaCmd = connection.CreateCommand())
        {
            pragmaCmd.Transaction = transaction;
            pragmaCmd.CommandText = "PRAGMA table_info(IdSubmissions);";

            bool columnExists = false;
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string existingCol = reader.GetString(1);
                    if (existingCol.Equals("photo_png", StringComparison.OrdinalIgnoreCase))
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                using var alterCmd = connection.CreateCommand();
                alterCmd.Transaction = transaction;
                alterCmd.CommandText = "ALTER TABLE IdSubmissions ADD COLUMN photo_png BLOB NULL;";
                alterCmd.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }

    private static void ApplyVersion4(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        // Save default background asset from embedded resource if not exists
        int? bgAssetId = null;
        try
        {
            using var chkCmd = connection.CreateCommand();
            chkCmd.Transaction = transaction;
            chkCmd.CommandText = "SELECT id FROM TemplateAssets WHERE kind = 'DefaultFrontBackground' LIMIT 1;";
            object? existing = chkCmd.ExecuteScalar();
            if (existing != null)
            {
                bgAssetId = Convert.ToInt32(existing);
            }
            else
            {
                using var stream = typeof(DatabaseMigrator).Assembly.GetManifestResourceStream("IdGen.Data.Assets.default_front_bg.jpg");
                if (stream != null)
                {
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    byte[] bgBytes = ms.ToArray();

                    using var assetCmd = connection.CreateCommand();
                    assetCmd.Transaction = transaction;
                    assetCmd.CommandText = @"
                        INSERT INTO TemplateAssets (kind, mime_type, file_size, data, created_at)
                        VALUES ('DefaultFrontBackground', 'image/jpeg', @size, @data, @created);
                        SELECT last_insert_rowid();";
                    assetCmd.Parameters.AddWithValue("@size", bgBytes.Length);
                    assetCmd.Parameters.AddWithValue("@data", bgBytes);
                    assetCmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("O"));
                    object? result = assetCmd.ExecuteScalar();
                    if (result != null)
                    {
                        bgAssetId = Convert.ToInt32(result);
                    }
                }
            }
        }
        catch { }

        // Set current layout as default active template in Templates table if no template exists
        using (var countCmd = connection.CreateCommand())
        {
            countCmd.Transaction = transaction;
            countCmd.CommandText = "SELECT COUNT(1) FROM Templates;";
            long count = Convert.ToInt64(countCmd.ExecuteScalar());
            if (count == 0)
            {
                var frontDef = TemplateDefaults.CreateDefaultFront(bgAssetId);
                var backDef = TemplateDefaults.CreateDefaultBack();
                string frontJson = JsonSerializer.Serialize(frontDef);
                string backJson = JsonSerializer.Serialize(backDef);

                using var tmplCmd = connection.CreateCommand();
                tmplCmd.Transaction = transaction;
                tmplCmd.CommandText = @"
                    INSERT INTO Templates (orientation, front_json, back_json, updated_at)
                    VALUES ('Portrait', @frontJson, @backJson, @updatedAt);";
                tmplCmd.Parameters.AddWithValue("@frontJson", frontJson);
                tmplCmd.Parameters.AddWithValue("@backJson", backJson);
                tmplCmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));
                tmplCmd.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }
}
