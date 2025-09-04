using Microsoft.Extensions.AI;
namespace MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAbstractions;


public interface IToolProvider
{
    IEnumerable<AIFunction> GetTools();
}
