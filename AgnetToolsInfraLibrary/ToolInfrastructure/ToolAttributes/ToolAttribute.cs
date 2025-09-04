using System;

namespace MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAttributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ToolAttribute : Attribute
{
    /// Optional human-friendly name (defaults to method name)
    public string? Name { get; init; }

    /// Short description for the tool (shown to the model)
    public string? Description { get; init; }

    /// Optional, human-oriented parameter docs (e.g., "string city, CancellationToken (optional)")
    public string? InputParams { get; init; }

    /// Optional, human-oriented output docs (e.g., "string[]")
    public string? OutputParams { get; init; }

    public string? OnFailure { get; init; }
}
