namespace IdGen.Core.Services;

public interface ISettingsService
{
    Task<string> GetSettingAsync(string key, string defaultValue = "");
    Task SetSettingAsync(string key, string value);
    Task<(string PrincipalName, string SchoolName)> GetSchoolDefaultsAsync();
    Task SaveSchoolDefaultsAsync(string principalName, string schoolName);
    Task<(string SchoolName, string CampusName, string PrincipalName, string PrincipalTitle, string Semester, string SchoolYear)> GetAllSettingsAsync();
    Task SaveAllSettingsAsync(string schoolName, string campusName, string principalName, string principalTitle, string semester, string schoolYear);
}
