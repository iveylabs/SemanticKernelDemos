using Microsoft.SemanticKernel;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;
using SemanticKernelDemos.Helpers;


namespace SemanticKernelDemos.Plugins;

public class ManageChatPlugin
{
#nullable enable
    private static void ReportPluginInUse([CallerMemberName] string? functionName = null)
    {
        WeakReferenceMessenger.Default.Send(new PluginInUseMessage(new Dictionary<string, string>
        {
            ["pluginName"] = nameof(ManageChatPlugin),
            ["functionName"] = functionName ?? "Unknown"
        }));
    }
#nullable disable

    // Clear chat history
    [KernelFunction("clear_chat_histroy"),]
    public static void ClearChatHistory()
    {
        // Not actually going to clear the chat, because it's not worth the effort
        ReportPluginInUse();
    }
}
