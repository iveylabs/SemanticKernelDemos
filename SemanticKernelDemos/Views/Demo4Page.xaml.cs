﻿using System.Diagnostics;
using System.Net;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using SemanticKernelDemos.Contracts.Services;
using SemanticKernelDemos.Helpers;
using SemanticKernelDemos.Plugins;
using SemanticKernelDemos.ViewModels;

namespace SemanticKernelDemos.Views;

public sealed partial class Demo4Page : Page
{
    public Demo4ViewModel ViewModel
    {
        get;
    }
    public Kernel Kernel
    {
        get; private set;
    }
    private readonly ChatManager _chatManager;
    public ChatManager ChatManager => _chatManager;
    public readonly ILocalSettingsService _localSettingsService;
    private string _endpoint = string.Empty;
    private string _key = string.Empty;
    private string _chatDeployment = string.Empty;
    private string _chatModel = string.Empty;

    public Demo4Page()
    {
        ViewModel = App.GetService<Demo4ViewModel>();
        InitializeComponent();

        // Show a loading circle
        ShowLoading();

        _localSettingsService = App.GetService<ILocalSettingsService>();
        LoadSettings();

        // Create a kernel with Azure OpenAI chat completion
        var kernelBuilder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                endpoint: _endpoint,
                apiKey: _key,
                deploymentName: _chatDeployment,
                modelId: _chatModel
            ) ?? throw new InvalidOperationException("Kernel builder creation failed.");

        // Build the kernel
        try
        {
            Kernel = kernelBuilder.Build();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Kernel creation failed.", ex);
        }
        Debug.WriteLine($"Current directory: {Environment.CurrentDirectory}");

        // Import TravelPlugin from prompt directory
        var travelPlugin = Kernel.ImportPluginFromPromptDirectory("Prompts/TravelPlugin");

        // Initialise ChatManager
        _chatManager = new ChatManager(Kernel, travelPlugin);

        // Hide the loading circle
        HideLoading();

        // Send an initial message from the "bot"
        AddMessageToConversation(AuthorRole.Assistant, "Hello! Tell me what activities you like and what you budget is, and I'll try to suggest some destinations that you'll love 😊✈️🌍");
    }

    private void LoadSettings()
    {
        var endpoint = _localSettingsService.ReadSetting<string>("AOAIEndpoint");
        var key = _localSettingsService.ReadSetting<string>("AOAIKey");
        var chatDeployment = _localSettingsService.ReadSetting<string>("AOAIChatDeployment");
        var chatModel = _localSettingsService.ReadSetting<string>("AOAIChatModel");

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

    private void ClearChatButton_Click(object sender, RoutedEventArgs e)
    {
        // Clear chat history
        ClearChatHistory();
        ChatManager.ClearChatHistory();
        ClearChatButton.Visibility = Visibility.Collapsed;
    }

    // Ensure the input text box receives keyboard focus when it's loaded
    private void InputTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        InputTextBox.Focus(FocusState.Keyboard);
    }

    // Clear the visible chat history
    public void ClearChatHistory()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            ConversationList.Items.Clear();
            ClearChatButton.Visibility = Visibility.Collapsed;

            // Send an initial message from the "bot"
            AddMessageToConversation(AuthorRole.Assistant, "Hello! Tell me what activities you like and what you budget is, and I'll try to suggest some destinations that you'll love 😊✈️🌍");
        });
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
                var response = await _chatManager.SendMessageAsync(userInput, "InvokeAsyncDest");

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

    // Handle send button clicks
    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SendMessage();
    }
}
