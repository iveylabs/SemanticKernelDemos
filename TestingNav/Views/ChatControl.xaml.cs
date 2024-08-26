using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Windows.ApplicationModel.Chat;
using SemanticKernelDemos.Helpers;
using System.Net;
using SemanticKernelDemos.Contracts.Services;
using System.Security.Cryptography.X509Certificates;
using Windows.Devices.Sms;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.UI;
using System.Diagnostics;
using SemanticPluginForge;
using System.Reflection;

namespace SemanticKernelDemos.Views;

public class MessageItem
{
    public string Text
    {
        get; set;
    }
    public SolidColorBrush BubbleColour
    {
        get; set;
    }
    public SolidColorBrush TextColour
    {
        get; set;
    }
    public HorizontalAlignment Alignment
    {
        get; set;
    }
    public Thickness Margin
    {
        get; set;
    }
    public string ImageSource
    {
        get; set;
    }
}

public sealed partial class ChatControl : UserControl
{
    public Kernel Kernel { get; private set; }
    private ChatManager _chatManager;
    public ChatManager ChatManager => _chatManager;
    public readonly ILocalSettingsService _localSettingsService;
    private string _endpoint = string.Empty;
    private string _key = string.Empty;
    private string _chatDeployment = string.Empty;
    private string _chatModel = string.Empty;
    private bool _autoInvoke;

    public ChatControl()
    {
        InitializeComponent();

        // Show a loading circle
        ShowLoading();

        _localSettingsService = App.GetService<ILocalSettingsService>();
        LoadSettings();
        //CreateKernelBuilder();

        //Initialise ChatManager
        //_chatManager = new ChatManager(Kernel, _autoInvoke);

        // Hide the loading circle
        HideLoading();
    }

    // Handle send button clicks
    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SendMessage();
    }

    // Handle pressing enter from the input text box
    private void InputTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Enable the send button if the input text box isn't empty, otherwise disable
        if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            SendButton.IsEnabled = true;
        }
        else
        {
            SendButton.IsEnabled = false;
        }

        // Send message when the user hits enter as long as the input text box isn't empty
        if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            SendMessage();
        }
    }

    private async void SendMessage()
    {
        string userInput = InputTextBox.Text;

        // Should always be true, but just in case
        if (!string.IsNullOrWhiteSpace(userInput))
        {
            try
            {
                // Show in-progress
                InProgress(true);

                // Add user message to the chat view
                AddMessageToConversation(AuthorRole.User, userInput);

                // Send user message to the chat manager
                var response = await _chatManager.SendMessageAsync(userInput);

                // Display the completion response
                AddMessageToConversation(AuthorRole.Assistant, response);
            }
            catch (Exception e)
            {
                // Display the error message
                AddMessageToConversation(AuthorRole.Assistant, $"Sorry, something went wrong! Error: {e.Message}");
            }
            finally
            {
                InProgress(false);
            }
        }
    }

    // Add message to the chat view
    private void AddMessageToConversation(AuthorRole role, string message)
    {
        // Enable the clear chat button
        ClearChatButton.Visibility = Visibility.Visible;

        // Is the message from the user?
        bool isUserMessage = role.Equals(AuthorRole.User);

        MessageItem messageItem = new()
        {
            // Change speech bubble and text properties based on the sender
            Text = message,
            BubbleColour = isUserMessage ? new SolidColorBrush(Colors.LightBlue) : new SolidColorBrush(Colors.LightGreen),
            TextColour = isUserMessage ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.Black), // unlikely to ever be wanted, because it'll probably look terrible
            Alignment = isUserMessage ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Margin = isUserMessage ? new Thickness(50, 5, 10, 5) : new Thickness(10, 5, 50, 5),
            //ImageSource = isUserMessage ? "Assets/user.png" : "Assets/ai.png"
            ImageSource = isUserMessage ? "ms-appx:///Assets/user.png" : "ms-appx:///Assets/ai.png"
        };

        // Add the message to the chat view
        ConversationList.Items.Add(messageItem);

        // Force layout update
        ConversationList.UpdateLayout();

        // Handle scrolling
        ConversationScrollViewer.UpdateLayout();
        ConversationScrollViewer.ChangeView(null, ConversationScrollViewer.ScrollableHeight, null);

    }

    // Clear the visible chat history
    public void ClearChatHistory()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            ConversationList.Items.Clear();
            ClearChatButton.Visibility = Visibility.Collapsed;
        });
    }

    // Ensure the input text box receives keyboard focus when it's loaded
    private void InputTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        InputTextBox.Focus(FocusState.Keyboard);
    }

    private void ClearChatButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear visible history
        ClearChatHistory();

        // Hide the button
        ClearChatButton.Visibility = Visibility.Collapsed;
        ChatManager.ClearChatHistory();

        // Clear the kernel history
        ChatManager.ClearChatHistory();
    }

    // In-progress display
    private void InProgress(bool inProgress)
    {
        if (inProgress)
        {
            InputTextBox.IsEnabled = false;
            InputTextBox.Text = string.Empty;
            InputTextBox.PlaceholderText = "Thinking...";
            SendButton.IsEnabled = false;
            ResponseProgressBar.Visibility = Visibility.Visible;
        }
        else
        {
            InputTextBox.IsEnabled = true;
            ResponseProgressBar.Visibility = Visibility.Collapsed;
            InputTextBox.PlaceholderText = "Enter a message to begin";
            InputTextBox.Focus(FocusState.Keyboard);
        }
    }

    private void InitialiseKernel(IKernelBuilder builder)
    {
        // Initialise the kernel
        if (_endpoint != null && _key != null && _chatDeployment != null && _chatModel != null)
        {
            builder.AddAzureOpenAIChatCompletion(
                endpoint: _endpoint,
                apiKey: _key,
                deploymentName: _chatDeployment,
                modelId: _chatModel
            );
        }
        else
        {
            throw new InvalidOperationException("Required settings are not loaded");
        }

        // Build the kernel
        try
        {
            Kernel = builder.Build();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Kernel creation failed. {e.Message}");
        }
    }

    private void LoadSettings()
    {
        var endpoint = _localSettingsService.ReadSetting<string>("AOAIEndpoint");
        var key = _localSettingsService.ReadSetting<string>("AOAIKey");
        var chatDeployment = _localSettingsService.ReadSetting<string>("AOAIChatDeployment");
        var chatModel =  _localSettingsService.ReadSetting<string>("AOAIChatModel");
        var autoInvoke = _localSettingsService.ReadSetting<bool>("AutoInvoke");

        if (endpoint != null)
        {
            _endpoint = endpoint;
        }
        if (key != null)
        {
            _key = key;
        }
        if (chatDeployment != null)
        {
            _chatDeployment = chatDeployment;
        }
        if (chatModel != null)
        {
            _chatModel = chatModel;
        }
        _autoInvoke = autoInvoke;
    }

    private void ShowLoading()
    {
        LoadingRing.IsActive = true;
        LoadingRing.Visibility = Visibility.Visible;
    }

    private void HideLoading()
    {
        LoadingRing.IsActive = false;
        LoadingRing.Visibility = Visibility.Collapsed;
    }

    private void InitializeChatManager()
    {
        _chatManager = new ChatManager(Kernel, _autoInvoke);
    }

    // Initialise the kernel
    public void CreateKernelBuilder(IEnumerable<Type> pluginTypes)
    {
        // Initialize the kernel
        var kernelBuilder = Kernel.CreateBuilder();
        InitialiseKernel(kernelBuilder);

        // Add plugins dynamically
        foreach (var pluginType in pluginTypes)
        {
            try
            {
                // Use the AddFromType extension method
                var method = typeof(KernelExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .FirstOrDefault(m => m.Name == "AddFromType" && m.GetParameters().Length == 3 &&
                                         m.GetParameters()[0].ParameterType == typeof(ICollection<KernelPlugin>) &&
                                         m.GetParameters()[1].ParameterType == typeof(string) &&
                                         m.GetParameters()[2].ParameterType == typeof(IServiceProvider));

                if (method == null)
                {
                    throw new InvalidOperationException("AddFromType method not found.");
                }

                var genericMethod = method.MakeGenericMethod(pluginType);
                genericMethod.Invoke(null, new object[] { Kernel.Plugins, null, null });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to add plugin: {pluginType.Name}. Error: {ex.Message}");
            }
        }

        if (Kernel == null)
        {
            throw new InvalidOperationException("Kernel is not initialised");
        }
        // Initialize ChatManager after Kernel is built
        InitializeChatManager();
    }
}