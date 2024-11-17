namespace TextTableConverter.Core.Services.Interfaces
{
    public interface ISettingsService
    {
        string OpenAIApiKey { get; set; }
        string GoogleDriveCredentials { get; set; }
        bool IsDarkMode { get; set; }
        bool ShowToolbar { get; set; }
        string GetApiKey();
        void SaveApiKey(string apiKey);
        void ClearApiKey();
        string GetDefaultModel();
        void SaveDefaultModel(string model);
    }
}
