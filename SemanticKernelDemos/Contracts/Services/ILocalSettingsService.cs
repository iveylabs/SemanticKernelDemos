namespace SemanticKernelDemos.Contracts.Services;

public interface ILocalSettingsService
{
    Task<T?> ReadSettingAsync<T>(string key);

    T ReadSetting<T>(string key);

    Task SaveSettingAsync<T>(string key, T value);
}
