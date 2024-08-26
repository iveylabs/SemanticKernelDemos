using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Helpers;
using SemanticKernelDemos.Plugins;
using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

public sealed partial class Demo5Page : Page
{
    public Demo5ViewModel ViewModel
    {
        get;
    }

    public Demo5Page()
    {
        ViewModel = App.GetService<Demo5ViewModel>();
        InitializeComponent();

        // Pass the plugins to ChatControl
        var chatControl = (ChatControl)FindName("ChatControl");
        if (chatControl != null)
        {
            var plugins = new List<Type>
            {
                typeof(TimePlugin),
                typeof(ManageChatPlugin),
                typeof(WeatherPlugin)
            };
            chatControl.CreateKernelBuilder(plugins);
        }
    }
}
