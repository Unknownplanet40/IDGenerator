using IdGen.Core.Validation;
using Xunit;

namespace IdGen.Tests;

public class InputValidatorsTests
{
    [Fact]
    public void ValidateRegistration_AllValid_ReturnsSuccess()
    {
        var result = InputValidators.ValidateRegistration(
            studentNo: "202110055",
            lastName: "Dela Cruz",
            firstName: "Juan",
            middleName: "Protacio",
            email: "juan.delacruz@university.edu.ph",
            phone: "09171234567",
            username: "juandc",
            password: "SecurePassword123!",
            confirmPassword: "SecurePassword123!"
        );

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("202110055", true)]
    [InlineData("2021-10055", true)]
    [InlineData("202410055", true)]
    [InlineData("20211005", false)]
    [InlineData("2021100550", false)]
    [InlineData("abcdefghi", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidStudentNo_ChecksCorrectness(string? studentNo, bool expectedValid)
    {
        bool actual = InputValidators.IsValidStudentNo(studentNo);
        Assert.Equal(expectedValid, actual);
    }

    [Fact]
    public void ValidateRegistration_EmptyFields_ReturnsExpectedFieldErrors()
    {
        var result = InputValidators.ValidateRegistration(
            studentNo: "",
            lastName: "",
            firstName: "",
            middleName: null,
            email: "",
            phone: "",
            username: "",
            password: "",
            confirmPassword: ""
        );

        Assert.False(result.IsValid);
        Assert.NotNull(result.GetError("studentNo"));
        Assert.NotNull(result.GetError("lastName"));
        Assert.NotNull(result.GetError("firstName"));
        Assert.NotNull(result.GetError("email"));
        Assert.NotNull(result.GetError("phone"));
        Assert.NotNull(result.GetError("username"));
        Assert.NotNull(result.GetError("password"));
    }

    [Theory]
    [InlineData("09171234567", true)]
    [InlineData("+639171234567", true)]
    [InlineData("09999999999", true)]
    [InlineData("08123456789", false)]
    [InlineData("12345", false)]
    [InlineData("0917-123-4567", true)]
    [InlineData("invalid-phone", false)]
    public void IsValidPhPhone_ChecksCorrectness(string phone, bool expectedValid)
    {
        bool actual = InputValidators.IsValidPhPhone(phone);
        Assert.Equal(expectedValid, actual);
    }

    [Theory]
    [InlineData("student@domain.com", true)]
    [InlineData("student@univ.edu.ph", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@nodomain.com", false)]
    [InlineData("", false)]
    public void IsValidEmail_ChecksCorrectness(string email, bool expectedValid)
    {
        bool actual = InputValidators.IsValidEmail(email);
        Assert.Equal(expectedValid, actual);
    }

    [Fact]
    public void ValidateRegistration_ShortPassword_ReturnsError()
    {
        var result = InputValidators.ValidateRegistration(
            studentNo: "202110055",
            lastName: "Dela Cruz",
            firstName: "Juan",
            middleName: null,
            email: "juan@example.com",
            phone: "09171234567",
            username: "juandc",
            password: "short",
            confirmPassword: "short"
        );

        Assert.False(result.IsValid);
        Assert.Equal("Password must be at least 8 characters.", result.GetError("password"));
    }

    [Fact]
    public void ValidateRegistration_PasswordMismatch_ReturnsError()
    {
        var result = InputValidators.ValidateRegistration(
            studentNo: "202110055",
            lastName: "Dela Cruz",
            firstName: "Juan",
            middleName: null,
            email: "juan@example.com",
            phone: "09171234567",
            username: "juandc",
            password: "Password123!",
            confirmPassword: "DifferentPassword123!"
        );

        Assert.False(result.IsValid);
        Assert.Equal("Passwords do not match.", result.GetError("confirmPassword"));
    }

    [Fact]
    public void ValidateSection_ValidInput_ReturnsSuccess()
    {
        var result = InputValidators.ValidateSection("Grade 11", "STEM-A", "Prof. Santos");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateSection_EmptyFields_ReturnsErrors()
    {
        var result = InputValidators.ValidateSection("", "", "");
        Assert.False(result.IsValid);
        Assert.NotNull(result.GetError("yearLevel"));
        Assert.NotNull(result.GetError("sectionName"));
        Assert.NotNull(result.GetError("adviser"));
    }

    [Fact]
    public void ValidateIdSubmission_ValidInput_ReturnsSuccess()
    {
        var today = new DateOnly(2026, 11, 18);
        var birthDate = new DateOnly(2005, 4, 10);
        byte[] dummyPng = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        var result = InputValidators.ValidateIdSubmission(
            gender: "Male",
            birthDate: birthDate,
            today: today,
            address: "123 Main St, City",
            guardianName: "Maria Dela Cruz",
            guardianPhone: "09181234567",
            sectionId: 1,
            signaturePng: dummyPng,
            acceptInfoChecked: true
        );

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateIdSubmission_MissingSignatureAndUncheckedConsent_ReturnsErrors()
    {
        var today = new DateOnly(2026, 11, 18);
        var birthDate = new DateOnly(2005, 4, 10);

        var result = InputValidators.ValidateIdSubmission(
            gender: "Male",
            birthDate: birthDate,
            today: today,
            address: "123 Main St, City",
            guardianName: "Maria Dela Cruz",
            guardianPhone: "09181234567",
            sectionId: 1,
            signaturePng: null,
            acceptInfoChecked: false
        );

        Assert.False(result.IsValid);
        Assert.NotNull(result.GetError("signaturePng"));
        Assert.NotNull(result.GetError("acceptInfoChecked"));
    }

    [Fact]
    public void ValidateIdSubmission_FutureBirthDate_ReturnsError()
    {
        var today = new DateOnly(2026, 11, 18);
        var futureDate = new DateOnly(2027, 1, 1);
        byte[] dummyPng = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

        var result = InputValidators.ValidateIdSubmission(
            gender: "Female",
            birthDate: futureDate,
            today: today,
            address: "123 Main St",
            guardianName: "Jane Doe",
            guardianPhone: "09181234567",
            sectionId: 1,
            signaturePng: dummyPng,
            acceptInfoChecked: true
        );

        Assert.False(result.IsValid);
        Assert.Equal("Birth date cannot be in the future.", result.GetError("birthDate"));
    }
}
