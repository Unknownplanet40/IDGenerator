using IdGen.Core.Services;
using Xunit;

namespace IdGen.Tests;

public class AgeCalculatorTests
{
    [Fact]
    public void Calculate_BirthdayToday_ReturnsExactAge()
    {
        var birthDate = new DateOnly(2000, 5, 15);
        var asOfDate = new DateOnly(2020, 5, 15);

        int age = AgeCalculator.Calculate(birthDate, asOfDate);
        Assert.Equal(20, age);
    }

    [Fact]
    public void Calculate_BirthdayTomorrow_SubtractsOneYear()
    {
        var birthDate = new DateOnly(2000, 5, 15);
        var asOfDate = new DateOnly(2020, 5, 14);

        int age = AgeCalculator.Calculate(birthDate, asOfDate);
        Assert.Equal(19, age);
    }

    [Fact]
    public void Calculate_BirthdayYesterday_ReturnsExactAge()
    {
        var birthDate = new DateOnly(2000, 5, 15);
        var asOfDate = new DateOnly(2020, 5, 16);

        int age = AgeCalculator.Calculate(birthDate, asOfDate);
        Assert.Equal(20, age);
    }

    [Fact]
    public void Calculate_LeapDayBirthday_HandlesNonLeapYearsCorrectly()
    {
        var birthDate = new DateOnly(2000, 2, 29);

        // On Feb 28, 2001, birthday has not occurred yet
        int ageFeb28 = AgeCalculator.Calculate(birthDate, new DateOnly(2001, 2, 28));
        Assert.Equal(0, ageFeb28);

        // On March 1, 2001, birthday is reached
        int ageMar1 = AgeCalculator.Calculate(birthDate, new DateOnly(2001, 3, 1));
        Assert.Equal(1, ageMar1);

        // Leap year 2004 Feb 28
        int age2004Feb28 = AgeCalculator.Calculate(birthDate, new DateOnly(2004, 2, 28));
        Assert.Equal(3, age2004Feb28);

        // Leap year 2004 Feb 29
        int age2004Feb29 = AgeCalculator.Calculate(birthDate, new DateOnly(2004, 2, 29));
        Assert.Equal(4, age2004Feb29);
    }

    [Fact]
    public void Calculate_FutureBirthDate_ThrowsArgumentException()
    {
        var birthDate = new DateOnly(2026, 12, 1);
        var asOfDate = new DateOnly(2026, 1, 1);

        Assert.Throws<ArgumentException>(() => AgeCalculator.Calculate(birthDate, asOfDate));
    }
}
