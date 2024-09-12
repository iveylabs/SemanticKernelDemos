using System.Net;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Contracts.Services;
using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

public sealed partial class HomePage : Page
{
    private readonly ILocalSettingsService _localSettingsService;

    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();
        _localSettingsService = App.GetService<ILocalSettingsService>();
        LoadSettingsAsync();
    }

    private void SettingsLink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
        Frame.Navigate(typeof(SettingsPage));
    }

    private async void LoadSettingsAsync()
    {
        var endpoint = await _localSettingsService.ReadSettingAsync<string>("AOAIEndpoint");
        var chatDeployment = await _localSettingsService.ReadSettingAsync<string>("AOAIChatDeployment");
        var chatModel = await _localSettingsService.ReadSettingAsync<string>("AOAIChatModel");
        var autoInvoke = await _localSettingsService.ReadSettingAsync<string>("AutoInvoke");

        EndpointLabel.FontWeight = FontWeights.Bold;
        ChatDeploymentLabel.FontWeight = FontWeights.Bold;
        ChatModelLabel.FontWeight = FontWeights.Bold;
        AutoInvokeLabel.FontWeight = FontWeights.Bold;

        if (endpoint != null)
        {
            EndpointValue.Text = endpoint;
        }
        else if (endpoint == null)
        {
            EndpointValue.Text = "NOT YET SET";
        }

        if (chatDeployment != null)
        {
            ChatDeploymentValue.Text = chatDeployment;
        }
        else if (chatDeployment == null)
        {
            ChatDeploymentValue.Text = "NOT YET SET";
        }
        if (chatModel != null)
        {
            ChatModelValue.Text = chatModel;
        }
        else if (chatModel == null)
        {
            ChatModelValue.Text = "NOT YET SET";
        }
        
        AutoInvokeValue.Text = autoInvoke;

    }
}
