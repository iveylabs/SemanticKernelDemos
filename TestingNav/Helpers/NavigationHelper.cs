using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Views;

namespace SemanticKernelDemos.Helpers;

// Helper class to set the navigation target for a NavigationViewItem.
//
// Usage in XAML:
// <NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavigationHelper.NavigateTo="AppName.ViewModels.MainViewModel" />
//
// Usage in code:
// NavigationHelper.SetNavigateTo(navigationViewItem, typeof(MainViewModel).FullName);
public class NavigationHelper
{
    public static string GetNavigateTo(NavigationViewItem item) => (string)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, string value) => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavigationHelper), new PropertyMetadata(null));

    private readonly Frame _frame;

    public NavigationHelper(Frame frame)
    {
        _frame = frame;
    }

    public ChatControl GetChatControl()
    {
        if (_frame.Content is Page currentPage)
        {
            // Assuming ChatControl is named "ChatControl" in the XAML
            return currentPage.FindName("ChatControl") as ChatControl;
        }

        return null;
    }

    public static implicit operator NavigationHelper(Frame v)
    {
        throw new NotImplementedException();
    }
}
