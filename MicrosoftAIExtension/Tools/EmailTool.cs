using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAttributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftAIExtensionWithToolAttributes.Tools
{  
    public class EmailTool
    {
        [Tool(
            Name = "SendEmail",
            Description = "Send an email with the weather update to a specified person.",
            InputParams = "string person, string weatherDescription,string clothsToWear optional",
            OutputParams = "string confirmationMessage",
            OnFailure = "Return an error message if the input is invalid or if any error occurs."
        )]
        public string SendEmail(string person, string weatherDescription, string clothsToWear = null)
        {
            if (string.IsNullOrWhiteSpace(person))
                throw new ArgumentException("Person cannot be null or empty.", nameof(person));
            if (string.IsNullOrWhiteSpace(weatherDescription))
                throw new ArgumentException("Weather description cannot be null or empty.", nameof(weatherDescription));
            // Simulate sending an email
            return $"Email sent to {person} with weather update: {weatherDescription} and cloths to wear {clothsToWear}";
        }
    }
}
