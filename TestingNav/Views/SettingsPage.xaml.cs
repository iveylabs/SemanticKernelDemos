using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Contracts.Services;
using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

public sealed partial class SettingsPage : Page
{
    private readonly ILocalSettingsService _localSettingsService;
    public SettingsViewModel ViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        _localSettingsService = App.GetService<ILocalSettingsService>();
        LoadSettingsAsync();
    }

    private async void LoadSettingsAsync()
    {
        var endpoint = await _localSettingsService.ReadSettingAsync<string>("AOAIEndpoint");
        var key = await _localSettingsService.ReadSettingAsync<string>("AOAIKey");
        var chatDeployment = await _localSettingsService.ReadSettingAsync<string>("AOAIChatDeployment");
        var chatModel = await _localSettingsService.ReadSettingAsync<string>("AOAIChatModel");
        var autoInvoke = await _localSettingsService.ReadSettingAsync<bool>("AutoInvoke");

        if (endpoint != null)
        {
            Endpoint.Text = endpoint;
        }
        if (key != null)
        {
            Key.Password = key;
        }
        if (chatDeployment != null)
        {
            ChatDeployment.Text = chatDeployment;
        }
        if (chatModel != null)
        {
            ChatModel.Text = chatModel;
        }
        AutoInvokeCheckBox.IsChecked = autoInvoke;

    }

    private async void SaveSettings_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var endpoint = Endpoint.Text;
        var key = Key.Password;
        var chatDeployment = ChatDeployment.Text;
        var chatModel = ChatModel.Text;
        var autoInvoke = AutoInvokeCheckBox.IsChecked;

        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            await _localSettingsService.SaveSettingAsync("AOAIEndpoint", endpoint);
        }
        if (!string.IsNullOrWhiteSpace(key))
        {
            await _localSettingsService.SaveSettingAsync("AOAIKey", key);
        }
        if (!string.IsNullOrWhiteSpace(chatDeployment))
        {
            await _localSettingsService.SaveSettingAsync("AOAIChatDeployment", chatDeployment);
        }
        if (!string.IsNullOrWhiteSpace(chatModel))
        {
            await _localSettingsService.SaveSettingAsync("AOAIChatModel", chatModel);
        }
            await _localSettingsService.SaveSettingAsync("AutoInvoke", autoInvoke);
    }
}
