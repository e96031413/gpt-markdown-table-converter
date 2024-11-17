namespace TextTableConverter.Core.Interfaces
{
    public interface ISettingsService
    {
        /// <summary>
        /// 同步儲存設定
        /// </summary>
        void SaveSettings<T>(string key, T value);

        /// <summary>
        /// 同步讀取設定
        /// </summary>
        T LoadSettings<T>(string key);

        /// <summary>
        /// 非同步儲存設定
        /// </summary>
        Task SaveSettingsAsync<T>(string key, T value);

        /// <summary>
        /// 非同步讀取設定
        /// </summary>
        Task<T> LoadSettingsAsync<T>(string key);

        /// <summary>
        /// 刪除設定
        /// </summary>
        void DeleteSettings(string key);

        /// <summary>
        /// 非同步刪除設定
        /// </summary>
        Task DeleteSettingsAsync(string key);

        /// <summary>
        /// 檢查設定是否存在
        /// </summary>
        bool SettingsExists(string key);

        /// <summary>
        /// 非同步檢查設定是否存在
        /// </summary>
        Task<bool> SettingsExistsAsync(string key);
    }
}
