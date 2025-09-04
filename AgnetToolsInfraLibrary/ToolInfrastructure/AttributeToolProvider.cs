using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAbstractions;
using MicrosoftAIExtensionWithToolAttributes.ToolInfrastructure.ToolAttributes;


namespace MicrosoftAIExtension.Infrastructure;

public sealed class AttributeToolProvider : IToolProvider
{
    private readonly IServiceProvider _services;
    private readonly Assembly[] _assembliesToScan;

    /// <summary>
    /// You can pass explicit assemblies to scan. If none are provided, scans all loaded (non-dynamic) assemblies.
    /// Register with DI as:
    /// builder.Services.AddSingleton<IToolProvider, AttributeToolProvider>
    /// </summary>

    public AttributeToolProvider(IServiceProvider services)
    {
        _services = services;
        _assembliesToScan = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.GetName().Name is not null)
                .ToArray();
    }


    /// <summary>
    /// You can pass explicit assemblies to scan. If none are provided, scans all loaded (non-dynamic) assemblies.
    /// Register with DI as:
    /// builder.Services.AddSingleton<IToolProvider>(sp =>
    ///     new AttributeToolProvider(sp, typeof(SomeKnownType).Assembly));
    /// </summary>
    public AttributeToolProvider(IServiceProvider services, params Assembly[] assembliesToScan)
    {
        _services = services;
        _assembliesToScan = (assembliesToScan is { Length: > 0 })
            ? assembliesToScan
            : AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && a.GetName().Name is not null)
                .ToArray();
    }

    public IEnumerable<AIFunction> GetTools()
    {
        var tools = new List<AIFunction>();

        foreach (var asm in _assembliesToScan)
        {
            Type[] types;
            try { types = asm.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null)!.Cast<Type>().ToArray(); }

            foreach (var type in types.Where(t => t is { IsAbstract: false, IsGenericTypeDefinition: false }))
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                  .Where(m => m.GetCustomAttribute<ToolAttribute>() is not null);

                if (!methods.Any()) continue;

                object? instance = null;
                if (methods.Any(m => !m.IsStatic))
                {
                    // Construct via DI (resolves deps if registered)
                    //instance = ActivatorUtilities.CreateInstance(_services, type);
                    instance = _services.GetService(type) ?? ActivatorUtilities.GetServiceOrCreateInstance(_services, type);
                }

                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<ToolAttribute>()!;
                    var toolName = string.IsNullOrWhiteSpace(attr.Name) ? method.Name : attr.Name!.Trim();

                    var (paramDoc, returnDoc) = BuildDocsFromReflection(method, attr);
                    var description = CombineDescription(attr.Description, paramDoc, returnDoc);

                    // NOTE: If your Microsoft.Extensions.AI package has different FunctionFactory.Create overloads,
                    // adjust accordingly (some previews omit the 'name' arg).
                    var fn = AIFunctionFactory.Create(
                        method: method,
                        target: method.IsStatic ? null : instance,
                        name: toolName,
                        description: description
                    );

                    tools.Add(fn);
                }
            }
        }

        return tools;
    }

    private static (string paramDoc, string returnDoc) BuildDocsFromReflection(MethodInfo method, ToolAttribute attr)
    {
        var paramDoc = attr.InputParams ?? string.Join(", ",
            method.GetParameters()
                  .Where(p => p.ParameterType != typeof(CancellationToken))
                  .Select(p =>
                  {
                      var isOptional = p.HasDefaultValue || p.IsOptional;
                      var opt = isOptional ? " (optional)" : "";
                      return $"{Pretty(p.ParameterType)} {p.Name}{opt}";
                  }));

        var retType = UnwrapTask(method.ReturnType);
        var returnDoc = attr.OutputParams ?? (retType == typeof(void) ? "void" : Pretty(retType));
        return (paramDoc, returnDoc);

        static Type UnwrapTask(Type t)
        {
            if (typeof(Task).IsAssignableFrom(t))
            {
                if (t.IsGenericType) return t.GetGenericArguments()[0];
                return typeof(void);
            }
            return t;
        }

        static string Pretty(Type t)
        {
            if (t.IsArray) return $"{Pretty(t.GetElementType()!)}[]";
            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                var name = def.Name.Split('`')[0];
                var args = string.Join(", ", t.GetGenericArguments().Select(Pretty));
                return $"{name}<{args}>";
            }
            return t.Name;
        }
    }

    private static string CombineDescription(string? description, string paramDoc, string returnDoc)
    {
        var desc = string.IsNullOrWhiteSpace(description) ? "No description provided." : description!.Trim();
        return $"{desc} Parameters: {paramDoc}. Returns: {returnDoc}.";
    }
}
