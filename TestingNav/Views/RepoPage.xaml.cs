using Microsoft.UI.Xaml.Controls;

using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class RepoPage : Page
{
    public RepoViewModel ViewModel
    {
        get;
    }

    public RepoPage()
    {
        ViewModel = App.GetService<RepoViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }
}
