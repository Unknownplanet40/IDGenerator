using IdGen.Core.Security;
using Xunit;

namespace IdGen.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ValidPassword_GeneratesNonEmptyHashAndSalt()
    {
        string password = "SecretPassword123!";
        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
        Assert.Equal(PasswordHasher.DefaultIterations, iterations);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        string password = "MyP@ssword2026";
        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        bool result = PasswordHasher.VerifyPassword(password, hash, salt, iterations);
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        string password = "MyP@ssword2026";
        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        bool result = PasswordHasher.VerifyPassword("WrongP@ssword", hash, salt, iterations);
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_TamperedHashOrSalt_ReturnsFalse()
    {
        string password = "MyP@ssword2026";
        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        // Corrupted salt
        byte[] saltBytes = Convert.FromBase64String(salt);
        saltBytes[0] ^= 0xFF;
        string badSalt = Convert.ToBase64String(saltBytes);

        Assert.False(PasswordHasher.VerifyPassword(password, hash, badSalt, iterations));

        // Invalid base64
        Assert.False(PasswordHasher.VerifyPassword(password, "not-valid-base64", salt, iterations));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void HashPassword_NullOrWhitespace_ThrowsException(string? invalidPassword)
    {
        Assert.ThrowsAny<ArgumentException>(() => PasswordHasher.HashPassword(invalidPassword!));
    }
}
