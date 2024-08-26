using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.UI.Xaml.Controls;
using SemanticKernelDemos.Views;
using Windows.System.RemoteSystems;

namespace SemanticKernelDemos.Helpers;
public class ChatManager
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService? _chatCompletionService;
    private readonly OpenAIPromptExecutionSettings _promptExecutionSettings;
    private ChatHistory _history;
    private string? _pluginInfo;
    private string? _botResponse;

    public ChatHistory History
    {
        get => _history; set => _history = value;
    }

    // Constructor (autoInvoke)
    public ChatManager(Kernel kernel, bool autoInvoke)
    {
        // For reporting the plugin and function in use
        WeakReferenceMessenger.Default.Register<PluginInUseMessage>(this, (r, m) =>
        {
            _pluginInfo = $"Plugin: {m.Value["pluginName"]} | Function: {m.Value["functionName"]}";
        });

        _history = [];
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        if (autoInvoke)
        {
            _promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };
        }
        else
        {
            _promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
            };
        }
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    // Constructor (no autoInvoke)
    public ChatManager(Kernel kernel)
    {
        // For reporting the plugin and function in use
        WeakReferenceMessenger.Default.Register<PluginInUseMessage>(this, (r, m) =>
        {
            _pluginInfo = $"Plugin: {m.Value["pluginName"]} | Function: {m.Value["functionName"]}";
        });

        _history = [];
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };
        _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    // Constructor (method)
    public ChatManager(Kernel kernel, string method)
    {
        // For reporting the plugin and function in use
        WeakReferenceMessenger.Default.Register<PluginInUseMessage>(this, (r, m) =>
        {
            _pluginInfo = $"Plugin: {m.Value["pluginName"]} | Function: {m.Value["functionName"]}";
        });

        _history = [];
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _promptExecutionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions
        };
    }

    // Clear the kernel history
    public void ClearChatHistory() => _history.RemoveRange(1, _history.Count - 1);

    // Send message and process response
    public async Task<string> SendMessageAsync(string message)
    {
        // Add user message to history and to chat view
        _history.AddMessage(AuthorRole.User, message);

        // Get the response from the chat completion service
        var response = await _chatCompletionService.GetChatMessageContentAsync(
            _history,
            _promptExecutionSettings,
            _kernel);

        if (response.Content != null)
        {
            // Store the current plugin info
            var currentPluginInfo = _pluginInfo;

            // Reset the plugin info for the next message
            _pluginInfo = string.Empty;

            // Prepend plugin and function info (if applicable) to the AI response
            _botResponse = !string.IsNullOrEmpty(currentPluginInfo)
                ? $"{currentPluginInfo}\n{response.Content}"
                : response.Content;
        }
        else
        {
            _botResponse = "Sorry, something isn't working! 😟 Are you trying to invoke a function without allowing auto-invocation?";
        }
        return _botResponse;
    }

    // Send message (method)
    public async Task<string> SendMessageAsync(string message, string method)
    {
        // Add user message to history and to chat view
        _history.AddMessage(AuthorRole.User, message);

        // Get the response from the chat completion service
        switch (method)
        {
            case ("InvokePromptAsync"):
                var promptResponse = await _kernel.InvokePromptAsync(message);
                if (promptResponse != null)
                {
                    _botResponse =  promptResponse.ToString();
                }
                else
                {
                    _botResponse = "Sorry, something isn't working! 😟 Are you trying to invoke a function without allowing auto-invocation?";
                }
                break;
            case ("InvokeAsync"):
                var invokeResponse = await _kernel.InvokeAsync("TimePlugin", "DayOfWeek");
                if (invokeResponse != null)
                {
                    _botResponse = invokeResponse.ToString();
                }
                else
                {
                    _botResponse = "Sorry, something isn't working! 😟 Are you trying to invoke a function without allowing auto-invocation?";
                }
                break;
            default:
                break;
        }

        //// Get the response from the chat completion service
        //var response = await _chatCompletionService.GetChatMessageContentAsync(
        //    _history,
        //    _promptExecutionSettings,
        //    _kernel);

        //if (response.Content != null)
        //{
        //    // Store the current plugin info
        //    var currentPluginInfo = _pluginInfo;

        //    // Reset the plugin info for the next message
        //    _pluginInfo = string.Empty;

        //    // Prepend plugin and function info (if applicable) to the AI response
        //    _botResponse = !string.IsNullOrEmpty(currentPluginInfo)
        //        ? $"{currentPluginInfo}\n{response.Content}"
        //        : response.Content;
        //}
        //else
        //{
        //    _botResponse = "Sorry, something isn't working! 😟 Are you trying to invoke a function without allowing auto-invocation?";
        //}
        return _botResponse;
    }
}