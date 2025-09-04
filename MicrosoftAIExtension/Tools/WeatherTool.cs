using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAttributes;
using System.Text.Json;

namespace MicrosoftAIExtension.Tools;

public class WeatherTool(string apiKey)
{
    private readonly HttpClient httpClient = new();
    private readonly string _apikey = apiKey;

    [Tool(
        Name = "GetWeatherInCity",
        Description = "Get the current weather descriptions in a specified city",
        InputParams = "string city, CancellationToken (optional)",
        OutputParams = "string[]",
        OnFailure = "Return an error message if the city is invalid or the API call fails."
    )]
    public async Task<string[]> GetWeatherInCity(string City, CancellationToken cancellationToken =default)
    {
        if (string.IsNullOrWhiteSpace(City))
            throw new ArgumentException("City cannot be null or empty.", nameof(City));
        
        if (string.IsNullOrWhiteSpace(_apikey))
            throw new InvalidOperationException("WEATHER_API_KEY environment variable is not set.");

        var url = $"https://api.weatherapi.com/v1/current.json?key={_apikey}&q={Uri.EscapeDataString(City)}&aqi=no";

        var response = await httpClient.GetAsync(url, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)            
            throw new InvalidOperationException($"Failed to get weather data: {responseContent}");

        using var jsonDoc = JsonDocument.Parse(responseContent);
        var root = jsonDoc.RootElement;
        var descriptionElement = root.GetProperty("current").GetProperty("condition").GetProperty("text");

        string[] description = [descriptionElement.GetString()!];
        return description;       
    }
}
