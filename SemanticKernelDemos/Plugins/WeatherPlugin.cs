using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using CommunityToolkit.Mvvm.Messaging;
using System.Runtime.CompilerServices;
using SemanticKernelDemos.Helpers;
using System.ComponentModel;
using System.Net;
using SemanticKernelDemos.Contracts.Services;
using Newtonsoft.Json.Linq;
using Azure.Core;
using System.Diagnostics;

namespace SemanticKernelDemos.Plugins;

public class WeatherPlugin
{
#nullable enable
    private static void ReportPluginInUse([CallerMemberName] string? functionName = null)
    {
        WeakReferenceMessenger.Default.Send(new PluginInUseMessage(new Dictionary<string, string>
        {
            ["pluginName"] = nameof(WeatherPlugin),
            ["functionName"] = functionName ?? "Unknown"
        }));
    }
#nullable disable

    // Get the weather based on a provided location
    [KernelFunction("get_weather"),
        Description("Provide the current weather in a given town or city")]
    public static async Task<string> GetWeather(
        [Description("The town or city for which the weather should be provided")] string location)
    {
        ReportPluginInUse();

        var localSettingsService = App.GetService<ILocalSettingsService>();
        var weatherAPIKey = localSettingsService.ReadSettingAsync<string>("WeatherAPIKey").Result;

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={weatherAPIKey}&units=metric";

        using var client = new HttpClient();
        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        RestResponse restResponse = new()
        {
            Weather = $"{json["weather"]?[0]?["description"]} with a temperature of {json["main"]?["temp"]} degrees Celsius."
        };
        var weather = restResponse.Weather.ToString();

        return weather;
    }
}
public class RestResponse
{
    public string Weather
    {
        get; set;
    }
    public string Result
    {
        get; set;
    }
    public string? Method
    {
        get; set;
    }
    public string? Uri
    {
        get; set;
    }
    public string? Body
    {
        get; set;
    }
}
