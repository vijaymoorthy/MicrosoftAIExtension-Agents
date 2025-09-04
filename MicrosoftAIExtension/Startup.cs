using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftAIExtension.Infrastructure;
using MicrosoftAIExtension.Tools;
using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAbstractions;

public static class Startup
{
    public static void ConfigureServices(HostApplicationBuilder hostApplicationBuilder,string provider, string model)
    {

        hostApplicationBuilder.Services.AddLogging(logging=>logging.AddConsole().SetMinimumLevel(LogLevel.Information));
        hostApplicationBuilder.Services.AddSingleton<ILoggerFactory>(sp => LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        }));

        hostApplicationBuilder.Services.AddTransient<WeatherTool>(_ =>
        {
            var weatherApiKey = Environment.GetEnvironmentVariable("WEATHERSERVICE_API_KEY");
            return new WeatherTool(weatherApiKey ?? throw new InvalidOperationException("WEATHERSERVICE_API_KEY environment variable is not set"));
        });


        //various models,tools 
        hostApplicationBuilder.Services.AddSingleton<IChatClient>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var client = provider switch
            {
                "openai" => new OpenAI.Chat.ChatClient(
                            model,
                            apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY")).AsIChatClient(),

                "gemini" => new GeminiChatClient(new GeminiDotnet.GeminiClientOptions
                            { ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
                              ModelId = model,
                              ApiVersion = GeminiApiVersions.V1Beta
                            }),

                _ => throw new NotSupportedException($"Unknown Provider '{provider}'")
            };

            return new ChatClientBuilder(client)
                .UseLogging(loggerFactory)
                .UseFunctionInvocation(loggerFactory,c=>    
                {
                    c.IncludeDetailedErrors = true;
                })
                .Build(sp);
        });


        //Tools registration using reflection and attributes
        hostApplicationBuilder.Services.AddSingleton<IToolProvider, AttributeToolProvider>();       

        hostApplicationBuilder.Services.AddTransient<ChatOptions>(sp =>
        {
            return new ChatOptions()
            {
                Tools = [..FunctionRegistry.GetTools(sp)],
                ModelId = model,
                Temperature = 0.7f,
                MaxOutputTokens = 500
            };
        });

       
    }
}
