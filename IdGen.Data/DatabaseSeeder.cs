using System.IO;
using System.Text.Json;
using IdGen.Data.Services;
using Microsoft.Data.Sqlite;

namespace IdGen.Data;

public static class DatabaseSeeder
{
    public static void SeedAppDefaults(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        // 1. Seed Sections if none exist
        using (var countCmd = connection.CreateCommand())
        {
            countCmd.Transaction = transaction;
            countCmd.CommandText = "SELECT COUNT(1) FROM Sections;";
            long count = Convert.ToInt64(countCmd.ExecuteScalar());
            if (count == 0)
            {
                const string seedSectionsSql = @"
                    INSERT OR IGNORE INTO Sections (course, year_level, section_name, adviser) VALUES
                    ('BSIT', '1st Year', '1-A', 'Prof. Michael Chang'),
                    ('BSIT', '1st Year', '1-B', 'Prof. Sarah Jenkins'),
                    ('BSIT', '2nd Year', '2-A', 'Dr. Robert Garcia'),
                    ('BSIT', '2nd Year', '2-B', 'Prof. Jessica Tan'),
                    ('BSIT', '3rd Year', '3-A', 'Engr. David Miller'),
                    ('BSIT', '3rd Year', '3-B', 'Prof. Amanda Ross'),
                    ('BSIT', '4th Year', '4-A', 'Dr. Christopher Lee'),
                    ('BSCS', '1st Year', '1-A', 'Prof. Alan Vance'),
                    ('BSCS', '2nd Year', '2-A', 'Dr. Evelyn Cruz'),
                    ('BSCS', '3rd Year', '3-A', 'Prof. Kenneth Torres'),
                    ('BSCS', '4th Year', '4-A', 'Dr. Gregory House'),
                    ('BSEMC', '1st Year', '1-A', 'Prof. Patricia Reyes'),
                    ('BSEMC', '2nd Year', '2-A', 'Prof. Marcus Bell'),
                    ('BSEMC', '3rd Year', '3-A', 'Prof. Laura Diaz'),
                    ('BSIS', '1st Year', '1-A', 'Dr. Raymond Holt'),
                    ('BSIS', '2nd Year', '2-A', 'Prof. Gina Linetti'),
                    ('BSIS', '3rd Year', '3-A', 'Prof. Terry Jeffords');
                ";
                using var seedCmd = connection.CreateCommand();
                seedCmd.Transaction = transaction;
                seedCmd.CommandText = seedSectionsSql;
                seedCmd.ExecuteNonQuery();
            }
        }

        // 2. Seed Settings with clean made-up default institution details if empty or default
        void EnsureSetting(string key, string defaultValue)
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = "SELECT value FROM Settings WHERE key = @key LIMIT 1;";
            cmd.Parameters.AddWithValue("@key", key);
            object? val = cmd.ExecuteScalar();
            string strVal = val?.ToString() ?? "";
            if (string.IsNullOrWhiteSpace(strVal) || strVal == "Cavite State University" || strVal == "Dr. Hernando D. Robles" || strVal == "2024-2025")
            {
                using var setCmd = connection.CreateCommand();
                setCmd.Transaction = transaction;
                setCmd.CommandText = "INSERT OR REPLACE INTO Settings (key, value) VALUES (@key, @val);";
                setCmd.Parameters.AddWithValue("@key", key);
                setCmd.Parameters.AddWithValue("@val", defaultValue);
                setCmd.ExecuteNonQuery();
            }
        }

        EnsureSetting("school_name", "Apex Institute of Science & Technology");
        EnsureSetting("campus_name", "Metropolitan Campus");
        EnsureSetting("principal_name", "Dr. Eleanor M. Vance");
        EnsureSetting("principal_title", "University Registrar");
        EnsureSetting("semester", "First");
        EnsureSetting("school_year", "2026-2027");

        // 3. Ensure Default Front Background Asset exists
        int? bgAssetId = null;
        using (var chkCmd = connection.CreateCommand())
        {
            chkCmd.Transaction = transaction;
            chkCmd.CommandText = "SELECT id FROM TemplateAssets WHERE kind = 'DefaultFrontBackground' LIMIT 1;";
            object? existing = chkCmd.ExecuteScalar();
            if (existing != null)
            {
                bgAssetId = Convert.ToInt32(existing);
            }
            else
            {
                try
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
                catch { }
            }
        }

        // 4. Update active template in Templates with current layout and default background
        var frontDef = TemplateDefaults.CreateDefaultFront(bgAssetId);
        var backDef = TemplateDefaults.CreateDefaultBack();
        string frontJson = JsonSerializer.Serialize(frontDef);
        string backJson = JsonSerializer.Serialize(backDef);

        using (var tmplCmd = connection.CreateCommand())
        {
            tmplCmd.Transaction = transaction;
            tmplCmd.CommandText = @"
                INSERT INTO Templates (orientation, front_json, back_json, updated_at)
                VALUES ('Portrait', @frontJson, @backJson, @updatedAt);";
            tmplCmd.Parameters.AddWithValue("@frontJson", frontJson);
            tmplCmd.Parameters.AddWithValue("@backJson", backJson);
            tmplCmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow.ToString("O"));
            tmplCmd.ExecuteNonQuery();
        }

        transaction.Commit();
    }
}
