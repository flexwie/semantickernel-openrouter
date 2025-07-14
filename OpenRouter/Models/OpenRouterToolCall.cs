using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Represents a tool call in OpenRouter API responses.
/// </summary>
public sealed class OpenRouterToolCall
{
    /// <summary>
    /// The ID of the tool call.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of the tool call (always "function" for function calls).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// The function call details.
    /// </summary>
    [JsonPropertyName("function")]
    public OpenRouterFunctionCall Function { get; set; } = new();
}

/// <summary>
/// Represents the function call details within a tool call.
/// </summary>
public sealed class OpenRouterFunctionCall
{
    /// <summary>
    /// The name of the function to call.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The arguments to call the function with, as a JSON string.
    /// </summary>
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// Represents a tool definition in OpenRouter API requests.
/// </summary>
public sealed class OpenRouterTool
{
    /// <summary>
    /// The type of the tool (always "function" for function tools).
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "function";

    /// <summary>
    /// The function definition.
    /// </summary>
    [JsonPropertyName("function")]
    public OpenRouterFunction Function { get; set; } = new();
}

/// <summary>
/// Represents tool choice configuration for OpenRouter API requests.
/// </summary>
public sealed class OpenRouterToolChoice
{
    /// <summary>
    /// The type of tool choice ("auto", "none", or specific tool selection).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// For specific tool selection, the function details.
    /// </summary>
    [JsonPropertyName("function")]
    public OpenRouterToolChoiceFunction? Function { get; set; }
}

/// <summary>
/// Represents a specific function choice in tool choice configuration.
/// </summary>
public sealed class OpenRouterToolChoiceFunction
{
    /// <summary>
    /// The name of the function to require.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}