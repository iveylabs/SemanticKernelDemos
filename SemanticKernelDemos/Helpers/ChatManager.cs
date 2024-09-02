using CommunityToolkit.Mvvm.Messaging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelDemos.Helpers;

public class ChatManager
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService? _chatCompletionService;
    private readonly OpenAIPromptExecutionSettings _promptExecutionSettings;
    private ChatHistory _history;
    private string? _pluginInfo;
    private string? _botResponse;
    private readonly KernelPlugin? _plugin;

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

    // Constructor (plugin)
    public ChatManager(Kernel kernel, KernelPlugin plugin)
    {
        _plugin = plugin;
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
    //public void ClearChatHistory() => _history.RemoveRange(1, _history.Count - 1);
    public void ClearChatHistory() => _history.Clear();

    // Send initial message from "bot"
    public async void SendIntroMessageAsync(string message)
    {
        // Add user message to history and to chat view
        _history.AddMessage(AuthorRole.Assistant, message);

        // Get the response from the chat completion service
        await _chatCompletionService.GetChatMessageContentAsync(
            _history,
            _promptExecutionSettings,
            _kernel);
    }

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
                    _botResponse = "Sorry, something isn't working! 😟 The LLM didn't respond";
                }
                break;
            case ("InvokeAsync"):
                var invokeResponse = await _kernel.InvokeAsync("TimePlugin", message);
                if (invokeResponse != null)
                {
                    _botResponse = invokeResponse.ToString();
                }
                else
                {
                    _botResponse = "Sorry, something isn't working! 😟 The LLM didn't respond";
                }
                break;
            case("InvokePromptAsyncWithTemplate"):
                var language = "French";
                var userBackground = message;

                var prompt = @"You are a travel assistant. You are helpful, creative, and very friendly.
                                Consider the traveller's background:
                                {{ConversationSummaryPlugin.SummarizeConversation $history}}
        
                                Create a list of helpful words and phrases in {language} the traveller would find useful.
        
                                Group phrases by category. Include common direction words. Display the 
                                phrases in the following format with English first, then French:
                                Hello - Ciao [chow]

                                Begin with: 'Here are some phrases in {{$language}} you may find helpful:' 
                                and end with: 'I hope this helps you on your trip!'";
                var templateResponse = await _kernel.InvokePromptAsync(prompt,
                    new()
                    {
                        { "language", language },
                        { "history", userBackground }
                    });
                if (templateResponse != null)
                {
                    _botResponse = templateResponse.ToString();
                }
                else
                {
                    _botResponse = "Sorry, something isn't working! 😟 The LLM didn't respond";
                }
                break;
            case ("InvokeAsyncDest"):
                var destResponse = await _kernel.InvokeAsync<string>(_plugin["SuggestDestinations"],
                    new()
                    {
                        { "input", message }
                    });
                if (destResponse != null)
                {
                    _botResponse = destResponse.ToString();
                }
                else
                {
                    _botResponse = "Sorry, something isn't working! 😟 The LLM didn't respond";
                }
                break;
            default:
                break;
        }
        return _botResponse;
    }
}