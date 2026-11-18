namespace IdGen.Core.Services;

public static class AgeCalculator
{
    public static int Calculate(DateOnly birthDate, DateOnly asOfDate)
    {
        if (birthDate > asOfDate)
        {
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        }

        int age = asOfDate.Year - birthDate.Year;

        // If haven't reached birthday in current year, subtract 1
        if (asOfDate.Month < birthDate.Month ||
            (asOfDate.Month == birthDate.Month && asOfDate.Day < birthDate.Day))
        {
            age--;
        }

        return age;
    }
}
