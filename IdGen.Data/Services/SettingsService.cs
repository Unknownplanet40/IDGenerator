using IdGen.Core.Models;
using IdGen.Core.Services;
using IdGen.Data.Repositories;

namespace IdGen.Data.Services;

public class SettingsService : ISettingsService
{
    private readonly ISettingRepository _settingRepository;

    public SettingsService(ISettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        return await _settingRepository.GetValueAsync(key, defaultValue);
    }

    public async Task SetSettingAsync(string key, string value)
    {
        await _settingRepository.SetValueAsync(key, value);
    }

    public async Task<(string PrincipalName, string SchoolName)> GetSchoolDefaultsAsync()
    {
        string principal = await _settingRepository.GetValueAsync(Setting.PrincipalNameKey, "");
        string school = await _settingRepository.GetValueAsync(Setting.SchoolNameKey, "");
        return (principal, school);
    }

    public async Task SaveSchoolDefaultsAsync(string principalName, string schoolName)
    {
        await _settingRepository.SetValueAsync(Setting.PrincipalNameKey, principalName.Trim());
        await _settingRepository.SetValueAsync(Setting.SchoolNameKey, schoolName.Trim());
    }

    public async Task<(string SchoolName, string CampusName, string PrincipalName, string PrincipalTitle, string Semester, string SchoolYear)> GetAllSettingsAsync()
    {
        string school = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.SchoolName, "Apex Institute of Science & Technology");
        string campus = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.CampusName, "Metropolitan Campus");
        string principal = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.PrincipalName, "Dr. Eleanor M. Vance");
        string title = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.PrincipalTitle, "University Registrar");
        string semester = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.Semester, "First");
        string schoolYear = await _settingRepository.GetValueAsync(IdGen.Core.Constants.SettingKeys.SchoolYear, "2026-2027");

        return (school, campus, principal, title, semester, schoolYear);
    }

    public async Task SaveAllSettingsAsync(string schoolName, string campusName, string principalName, string principalTitle, string semester, string schoolYear)
    {
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.SchoolName, schoolName.Trim());
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.CampusName, campusName?.Trim() ?? string.Empty);
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.PrincipalName, principalName.Trim());
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.PrincipalTitle, principalTitle.Trim());
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.Semester, semester.Trim());
        await _settingRepository.SetValueAsync(IdGen.Core.Constants.SettingKeys.SchoolYear, schoolYear.Trim());
    }
}
