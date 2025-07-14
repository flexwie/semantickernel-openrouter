using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using SemanticKernel.Connectors.OpenRouter.Models;

namespace SemanticKernel.Connectors.OpenRouter.Core;

/// <summary>
/// Utilities for converting between Semantic Kernel functions and OpenRouter function format.
/// </summary>
public static class OpenRouterFunctionHelpers
{
    /// <summary>
    /// The default function name separator.
    /// </summary>
    private const string DefaultFunctionNameSeparator = "-";

    /// <summary>
    /// Converts a collection of Semantic Kernel functions to OpenRouter tools.
    /// </summary>
    /// <param name="functions">The functions to convert.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <returns>An array of OpenRouter tools.</returns>
    public static OpenRouterTool[] ConvertToOpenRouterTools(
        IEnumerable<KernelFunction> functions,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        return functions.Select(f => ConvertToOpenRouterTool(f, functionNameSeparator)).ToArray();
    }

    /// <summary>
    /// Converts a single Semantic Kernel function to an OpenRouter tool.
    /// </summary>
    /// <param name="function">The function to convert.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <returns>An OpenRouter tool.</returns>
    public static OpenRouterTool ConvertToOpenRouterTool(
        KernelFunction function,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        return new OpenRouterTool
        {
            Type = "function",
            Function = ConvertToOpenRouterFunction(function, functionNameSeparator)
        };
    }

    /// <summary>
    /// Converts a Semantic Kernel function to an OpenRouter function.
    /// </summary>
    /// <param name="function">The function to convert.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <returns>An OpenRouter function.</returns>
    public static OpenRouterFunction ConvertToOpenRouterFunction(
        KernelFunction function,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        var functionName = CreateFunctionName(function, functionNameSeparator);
        
        return new OpenRouterFunction
        {
            Name = functionName,
            Description = function.Description ?? string.Empty,
            Parameters = ConvertParametersToSchema(function.Metadata.Parameters)
        };
    }

    /// <summary>
    /// Creates a function name by combining plugin name and function name.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <returns>The combined function name.</returns>
    public static string CreateFunctionName(
        KernelFunction function,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        if (string.IsNullOrEmpty(function.PluginName))
        {
            return function.Name;
        }

        return $"{function.PluginName}{functionNameSeparator}{function.Name}";
    }

    /// <summary>
    /// Parses a function name to extract plugin name and function name.
    /// </summary>
    /// <param name="functionName">The combined function name.</param>
    /// <param name="functionNameSeparator">The separator used between plugin and function names.</param>
    /// <returns>A tuple containing the plugin name and function name.</returns>
    public static (string? PluginName, string FunctionName) ParseFunctionName(
        string functionName,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        var separatorIndex = functionName.IndexOf(functionNameSeparator, StringComparison.Ordinal);
        
        if (separatorIndex == -1)
        {
            return (null, functionName);
        }

        var pluginName = functionName[..separatorIndex];
        var actualFunctionName = functionName[(separatorIndex + functionNameSeparator.Length)..];
        
        return (pluginName, actualFunctionName);
    }

    /// <summary>
    /// Converts function parameters to OpenRouter function parameters schema.
    /// </summary>
    /// <param name="parameters">The function parameters.</param>
    /// <returns>The OpenRouter function parameters schema.</returns>
    private static OpenRouterFunctionParameters ConvertParametersToSchema(IReadOnlyList<KernelParameterMetadata> parameters)
    {
        var schema = new OpenRouterFunctionParameters();

        foreach (var parameter in parameters)
        {
            var parameterSchema = ConvertParameterToSchema(parameter);
            schema.Properties[parameter.Name] = parameterSchema;
            
            if (parameter.IsRequired)
            {
                schema.Required.Add(parameter.Name);
            }
        }

        return schema;
    }

    /// <summary>
    /// Converts a single parameter to OpenRouter function property schema.
    /// </summary>
    /// <param name="parameter">The parameter metadata.</param>
    /// <returns>The OpenRouter function property.</returns>
    private static OpenRouterFunctionProperty ConvertParameterToSchema(KernelParameterMetadata parameter)
    {
        var property = new OpenRouterFunctionProperty
        {
            Description = parameter.Description
        };

        // Convert .NET type to JSON schema type
        var parameterType = parameter.ParameterType ?? typeof(string);
        property.Type = GetJsonSchemaType(parameterType);

        // Handle special cases
        if (parameterType.IsEnum)
        {
            property.Type = "string";
            property.Enum = Enum.GetNames(parameterType).ToList();
        }
        else if (parameterType.IsArray || (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(List<>)))
        {
            property.Type = "array";
            var elementType = parameterType.IsArray ? parameterType.GetElementType()! : parameterType.GetGenericArguments()[0];
            property.Items = new OpenRouterFunctionProperty
            {
                Type = GetJsonSchemaType(elementType)
            };
        }

        return property;
    }

    /// <summary>
    /// Converts a .NET type to a JSON schema type string.
    /// </summary>
    /// <param name="type">The .NET type.</param>
    /// <returns>The JSON schema type string.</returns>
    private static string GetJsonSchemaType(Type type)
    {
        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => "boolean",
            TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or 
            TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => "integer",
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => "number",
            TypeCode.String or TypeCode.Char => "string",
            TypeCode.DateTime => "string",
            _ => "string" // Default to string for unknown types
        };
    }

    /// <summary>
    /// Converts OpenRouter tool choice behavior to the appropriate tool_choice parameter.
    /// </summary>
    /// <param name="behavior">The function choice behavior.</param>
    /// <param name="functions">The available functions.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <returns>The tool choice parameter value.</returns>
    public static object? ConvertFunctionChoiceBehaviorToToolChoice(
        FunctionChoiceBehavior? behavior,
        IEnumerable<KernelFunction>? functions = null,
        string functionNameSeparator = DefaultFunctionNameSeparator)
    {
        if (behavior == null)
        {
            return null;
        }

        return behavior.GetType().Name switch
        {
            "AutoFunctionChoiceBehavior" => "auto",
            "NoneFunctionChoiceBehavior" => "none",
            "RequiredFunctionChoiceBehavior" when TryGetRequiredFunction(behavior, functions, functionNameSeparator, out var functionName) => 
                new { type = "function", function = new { name = functionName } },
            _ => null
        };
    }

    /// <summary>
    /// Tries to get the required function name from a RequiredFunctionChoiceBehavior.
    /// </summary>
    /// <param name="behavior">The function choice behavior.</param>
    /// <param name="functions">The available functions.</param>
    /// <param name="functionNameSeparator">The separator to use between plugin and function names.</param>
    /// <param name="functionName">The function name if found.</param>
    /// <returns>True if a required function was found; otherwise, false.</returns>
    private static bool TryGetRequiredFunction(
        FunctionChoiceBehavior behavior,
        IEnumerable<KernelFunction>? functions,
        string functionNameSeparator,
        out string? functionName)
    {
        functionName = null;

        // Use reflection to access the Functions property
        var functionsProperty = behavior.GetType().GetProperty("Functions");
        if (functionsProperty?.GetValue(behavior) is not IEnumerable<KernelFunction> requiredFunctions)
        {
            return false;
        }

        var firstFunction = requiredFunctions.FirstOrDefault();
        if (firstFunction == null)
        {
            return false;
        }

        functionName = CreateFunctionName(firstFunction, functionNameSeparator);
        return true;
    }
}