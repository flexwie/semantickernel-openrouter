using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Represents a function definition for OpenRouter API function calling.
/// </summary>
public sealed class OpenRouterFunction
{
    /// <summary>
    /// The name of the function to be called.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of what the function does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The parameters the function accepts, described as a JSON Schema object.
    /// </summary>
    [JsonPropertyName("parameters")]
    public OpenRouterFunctionParameters? Parameters { get; set; }
}

/// <summary>
/// Represents the parameters schema for an OpenRouter function.
/// </summary>
public sealed class OpenRouterFunctionParameters
{
    /// <summary>
    /// The type of the parameters object (always "object" for function parameters).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    /// <summary>
    /// The properties of the parameters object.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, OpenRouterFunctionProperty> Properties { get; set; } = new();

    /// <summary>
    /// The required parameter names.
    /// </summary>
    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = new();
}

/// <summary>
/// Represents a single parameter property in an OpenRouter function.
/// </summary>
public sealed class OpenRouterFunctionProperty
{
    /// <summary>
    /// The type of the parameter (e.g., "string", "number", "boolean", "array", "object").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// A description of the parameter.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// For array types, the schema of the items in the array.
    /// </summary>
    [JsonPropertyName("items")]
    public OpenRouterFunctionProperty? Items { get; set; }

    /// <summary>
    /// For object types, the properties of the object.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, OpenRouterFunctionProperty>? Properties { get; set; }

    /// <summary>
    /// For string types, an enumeration of valid values.
    /// </summary>
    [JsonPropertyName("enum")]
    public List<string>? Enum { get; set; }
}