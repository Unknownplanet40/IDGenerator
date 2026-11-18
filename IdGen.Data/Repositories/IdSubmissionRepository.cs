using IdGen.Core.Enums;
using IdGen.Core.Models;
using Microsoft.Data.Sqlite;

namespace IdGen.Data.Repositories;

public interface IIdSubmissionRepository
{
    Task<IdSubmission?> GetByIdAsync(int id);
    Task<IdSubmission?> GetByUserIdAsync(int userId);
    Task<IReadOnlyList<IdSubmission>> GetAllAsync();
    Task<IReadOnlyList<IdSubmission>> GetPendingAsync();
    Task<int> CreateAsync(IdSubmission submission);
    Task UpdateStatusAsync(int submissionId, SubmissionStatus status);
    Task DeleteByUserIdAsync(int userId);
    Task DeleteAsync(int id);
}

public class IdSubmissionRepository : IIdSubmissionRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        s.id, s.user_id, s.gender, s.birth_date, s.address, s.guardian_name, s.guardian_phone, 
        s.section_id, s.adviser_snapshot, s.principal_snapshot, s.school_snapshot, s.signature_png, 
        s.status, s.submitted_at, s.blood_type, s.course_snapshot, s.campus_snapshot, 
        s.principal_title_snapshot, s.semester_snapshot, s.school_year_snapshot, s.photo_png,
        u.student_no, u.first_name, u.last_name, u.middle_name";

    public IdSubmissionRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IdSubmission?> GetByIdAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM IdSubmissions s LEFT JOIN Users u ON s.user_id = u.id WHERE s.id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapSubmission(reader);
        }
        return null;
    }

    public async Task<IdSubmission?> GetByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM IdSubmissions s LEFT JOIN Users u ON s.user_id = u.id WHERE s.user_id = @userId LIMIT 1;";
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapSubmission(reader);
        }
        return null;
    }

    public async Task<IReadOnlyList<IdSubmission>> GetAllAsync()
    {
        var list = new List<IdSubmission>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM IdSubmissions s LEFT JOIN Users u ON s.user_id = u.id ORDER BY s.submitted_at DESC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSubmission(reader));
        }
        return list;
    }

    public async Task<IReadOnlyList<IdSubmission>> GetPendingAsync()
    {
        var list = new List<IdSubmission>();
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT {SelectColumns} FROM IdSubmissions s LEFT JOIN Users u ON s.user_id = u.id WHERE s.status = 'Pending' ORDER BY s.submitted_at ASC;";

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapSubmission(reader));
        }
        return list;
    }

    public async Task<int> CreateAsync(IdSubmission submission)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO IdSubmissions (
                user_id, gender, birth_date, address, guardian_name, guardian_phone, 
                section_id, adviser_snapshot, principal_snapshot, school_snapshot, 
                signature_png, status, submitted_at, blood_type, course_snapshot, 
                campus_snapshot, principal_title_snapshot, semester_snapshot, school_year_snapshot,
                photo_png
            )
            VALUES (
                @userId, @gender, @birthDate, @address, @guardianName, @guardianPhone, 
                @sectionId, @adviserSnapshot, @principalSnapshot, @schoolSnapshot, 
                @signaturePng, @status, @submittedAt, @bloodType, @courseSnapshot, 
                @campusSnapshot, @principalTitleSnapshot, @semesterSnapshot, @schoolYearSnapshot,
                @photoPng
            );
            SELECT last_insert_rowid();";

        cmd.Parameters.AddWithValue("@userId", submission.UserId);
        cmd.Parameters.AddWithValue("@gender", submission.Gender);
        cmd.Parameters.AddWithValue("@birthDate", submission.BirthDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@address", submission.Address);
        cmd.Parameters.AddWithValue("@guardianName", submission.GuardianName);
        cmd.Parameters.AddWithValue("@guardianPhone", submission.GuardianPhone);
        cmd.Parameters.AddWithValue("@sectionId", submission.SectionId);
        cmd.Parameters.AddWithValue("@adviserSnapshot", submission.AdviserSnapshot);
        cmd.Parameters.AddWithValue("@principalSnapshot", submission.PrincipalSnapshot);
        cmd.Parameters.AddWithValue("@schoolSnapshot", submission.SchoolSnapshot);
        cmd.Parameters.AddWithValue("@signaturePng", submission.SignaturePng);
        cmd.Parameters.AddWithValue("@status", submission.Status.ToString());
        cmd.Parameters.AddWithValue("@submittedAt", submission.SubmittedAt.ToString("O"));

        cmd.Parameters.AddWithValue("@bloodType", (object?)submission.BloodType ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@courseSnapshot", (object?)submission.CourseSnapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@campusSnapshot", (object?)submission.CampusSnapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@principalTitleSnapshot", (object?)submission.PrincipalTitleSnapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@semesterSnapshot", (object?)submission.SemesterSnapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@schoolYearSnapshot", (object?)submission.SchoolYearSnapshot ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@photoPng", (submission.PhotoPng != null && submission.PhotoPng.Length > 0) ? (object)submission.PhotoPng : DBNull.Value);

        object? result = await cmd.ExecuteScalarAsync();
        submission.Id = Convert.ToInt32(result);
        return submission.Id;
    }

    public async Task UpdateStatusAsync(int submissionId, SubmissionStatus status)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE IdSubmissions SET status = @status WHERE id = @id;";
        cmd.Parameters.AddWithValue("@status", status.ToString());
        cmd.Parameters.AddWithValue("@id", submissionId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteByUserIdAsync(int userId)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM IdSubmissions WHERE user_id = @userId;";
        cmd.Parameters.AddWithValue("@userId", userId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        using var conn = _connectionFactory.CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM IdSubmissions WHERE id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static IdSubmission MapSubmission(SqliteDataReader reader)
    {
        long bytesCount = reader.GetBytes(11, 0, null, 0, 0);
        byte[] sigBytes = new byte[bytesCount];
        reader.GetBytes(11, 0, sigBytes, 0, (int)bytesCount);

        byte[] photoBytes = Array.Empty<byte>();
        if (!reader.IsDBNull(20))
        {
            long photoBytesCount = reader.GetBytes(20, 0, null, 0, 0);
            photoBytes = new byte[photoBytesCount];
            reader.GetBytes(20, 0, photoBytes, 0, (int)photoBytesCount);
        }

        string? studentNo = null;
        if (!reader.IsDBNull(21)) studentNo = reader.GetString(21);

        string? studentName = null;
        if (!reader.IsDBNull(22) && !reader.IsDBNull(23))
        {
            string fn = reader.GetString(22);
            string ln = reader.GetString(23);
            string mn = reader.IsDBNull(24) ? "" : reader.GetString(24);
            studentName = string.IsNullOrWhiteSpace(mn) ? $"{fn} {ln}" : $"{fn} {mn} {ln}";
        }

        return new IdSubmission
        {
            Id = reader.GetInt32(0),
            UserId = reader.GetInt32(1),
            Gender = reader.GetString(2),
            BirthDate = DateOnly.Parse(reader.GetString(3)),
            Address = reader.GetString(4),
            GuardianName = reader.GetString(5),
            GuardianPhone = reader.GetString(6),
            SectionId = reader.GetInt32(7),
            AdviserSnapshot = reader.GetString(8),
            PrincipalSnapshot = reader.GetString(9),
            SchoolSnapshot = reader.GetString(10),
            SignaturePng = sigBytes,
            Status = Enum.Parse<SubmissionStatus>(reader.GetString(12)),
            SubmittedAt = DateTime.Parse(reader.GetString(13)),
            BloodType = reader.IsDBNull(14) ? null : reader.GetString(14),
            CourseSnapshot = reader.IsDBNull(15) ? null : reader.GetString(15),
            CampusSnapshot = reader.IsDBNull(16) ? null : reader.GetString(16),
            PrincipalTitleSnapshot = reader.IsDBNull(17) ? null : reader.GetString(17),
            SemesterSnapshot = reader.IsDBNull(18) ? null : reader.GetString(18),
            SchoolYearSnapshot = reader.IsDBNull(19) ? null : reader.GetString(19),
            PhotoPng = photoBytes,
            StudentNo = studentNo,
            StudentName = studentName
        };
    }
}
