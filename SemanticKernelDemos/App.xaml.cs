﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Activation;
using SemanticKernelDemos.Contracts.Services;
using SemanticKernelDemos.Core.Contracts.Services;
using SemanticKernelDemos.Core.Services;
using SemanticKernelDemos.Helpers;
using SemanticKernelDemos.Models;
using SemanticKernelDemos.Plugins;
using SemanticKernelDemos.Services;
using SemanticKernelDemos.ViewModels;
using SemanticKernelDemos.Views;

namespace SemanticKernelDemos;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    public static DispatcherQueue? MainDispatcherQueue { get; private set; }

    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static UIElement? AppTitlebar { get; set; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Register the Frame
            services.AddSingleton<Frame>();

            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<RepoViewModel>();
            services.AddTransient<RepoPage>();
            services.AddTransient<DocsViewModel>();
            services.AddTransient<DocsPage>();
            services.AddTransient<Demo1ViewModel>();
            services.AddTransient<Demo1Page>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomePage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Custom plugins
            services.AddTransient<ManageChatPlugin>();

            // Register ChatControl
            services.AddTransient<ChatControl>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        var mainWindow = App.MainWindow;
        mainWindow.Width = 2048;

        // Store the main dispatcher queue
        MainDispatcherQueue = DispatcherQueue.GetForCurrentThread();

        await App.GetService<IActivationService>().ActivateAsync(args);

    }
}
