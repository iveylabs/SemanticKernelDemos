using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SemanticKernelDemos.Helpers;
internal class PluginInUseMessage(Dictionary<string, string> value) : ValueChangedMessage<Dictionary<string, string>>(value);
