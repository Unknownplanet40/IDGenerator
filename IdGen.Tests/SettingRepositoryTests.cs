using IdGen.Core.Models;
using IdGen.Data.Repositories;
using Xunit;

namespace IdGen.Tests;

public class SettingRepositoryTests
{
    [Fact]
    public async Task GetAndSetSetting_PersistsAndUpdates()
    {
        using var factory = SqliteTestFixture.CreateInMemoryFactory();
        var repo = new SettingRepository(factory);

        // Get default seeded value
        string school = await repo.GetValueAsync(Setting.SchoolNameKey);
        Assert.Equal("", school);

        // Update value
        await repo.SetValueAsync(Setting.SchoolNameKey, "Cavite State University");
        string updatedSchool = await repo.GetValueAsync(Setting.SchoolNameKey);
        Assert.Equal("Cavite State University", updatedSchool);

        // Set principal
        await repo.SetValueAsync(Setting.PrincipalNameKey, "Dr. Hernando Robles");
        string principal = await repo.GetValueAsync(Setting.PrincipalNameKey);
        Assert.Equal("Dr. Hernando Robles", principal);

        // Non-existent key returns default
        string nonExistent = await repo.GetValueAsync("random_key", "default_val");
        Assert.Equal("default_val", nonExistent);
    }
}
