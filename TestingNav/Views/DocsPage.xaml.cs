using Microsoft.UI.Xaml.Controls;

using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

// To learn more about WebView2, see https://docs.microsoft.com/microsoft-edge/webview2/.
public sealed partial class DocsPage : Page
{
    public DocsViewModel ViewModel
    {
        get;
    }

    public DocsPage()
    {
        ViewModel = App.GetService<DocsViewModel>();
        InitializeComponent();

        ViewModel.WebViewService.Initialize(WebView);
    }
}
