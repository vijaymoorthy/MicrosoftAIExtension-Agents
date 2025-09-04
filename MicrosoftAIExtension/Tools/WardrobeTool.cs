using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAttributes;

namespace MicrosoftAIExtensionWithToolAttributes.Tools;

public class WardrobeTool
{
    [Tool(
        Name = "GetOutfitSuggestion",
        Description = "Get outfit suggestion based on the weather description",
        InputParams = "string weatherDescription",
        OutputParams = "string outfitSuggestion",
        OnFailure = "Return an error message if the input is invalid or if any error occurs."
    )]
    public string GetOutfitSuggestion(string weatherDescription)
    {
        if (string.IsNullOrWhiteSpace(weatherDescription))
            throw new ArgumentException("Weather description cannot be null or empty.", nameof(weatherDescription));
        return weatherDescription.ToLower() switch
        {
            var desc when desc.Contains("sunny") || desc.Contains("clear") => "A light t-shirt and shorts.",
            var desc when desc.Contains("rain") || desc.Contains("drizzle") => "A waterproof jacket and waterproof boots.",
            var desc when desc.Contains("snow") => "A warm coat, scarf, gloves, and insulated boots.",
            var desc when desc.Contains("cloudy") => "A long-sleeve shirt and jeans.",
            var desc when desc.Contains("windy") => "A windbreaker and layered clothing.",
            var desc when desc.Contains("fog") => "A light jacket and reflective gear.",
            var desc when desc.Contains("thunderstorm") => "A waterproof jacket, waterproof boots, and an umbrella.",
            var desc when desc.Contains("hot") || desc.Contains("warm") => "Lightweight clothing such as a tank top and shorts.",
            var desc when desc.Contains("cold") || desc.Contains("chilly") => "A warm sweater, coat, and scarf.",
            _ => "Check the weather forecast for more details."
        };
    }
}
