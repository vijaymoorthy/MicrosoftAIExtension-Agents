using dotenv.net;
using System;
using Microsoft.Extensions.Hosting;
using MicrosoftAIExtension.NewFolder;


DotEnv.Load();

//default provider and models
string provider = "openai";
string model = "gpt-4o";
for (int i = 0;i<args.Length;i++)
{
    if (args[i] == "--provider" && i + 1 < args.Length)
    {
        provider = args[i + 1].ToLower();
    }
    if (args[i] == "--model" && i + 1 < args.Length)
    {
        model = args[i + 1].ToLower();
    }
}

var builder = Host.CreateApplicationBuilder(args);
Startup.ConfigureServices(builder, provider, model);

var host = builder.Build();

await ChatAgent.RunAsync(host.Services);