using CommunityToolkit.Mvvm.ComponentModel;

namespace SemanticKernelDemos.ViewModels;

public partial class HomeViewModel : ObservableRecipient
{

    private string _homeHeading = "Welcome to my Semantic Kernel demo app!";

    public string HomeHeading
    {
        get => _homeHeading;
        set => SetProperty(ref _homeHeading, value);
    }

    public HomeViewModel()
    {
    }
}
