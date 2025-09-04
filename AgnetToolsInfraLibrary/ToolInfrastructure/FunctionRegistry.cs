using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAbstractions;

namespace MicrosoftAIExtension.Infrastructure;

public static class FunctionRegistry
{
    /// <summary>
    /// Existing call-site compatibility:
    ///   [.. FunctionRegistry.GetTools(sp)]
    /// </summary>
    public static IEnumerable<AIFunction> GetTools(IServiceProvider sp)
    {
        var provider = sp.GetRequiredService<IToolProvider>();
        return provider.GetTools();
    }
}
