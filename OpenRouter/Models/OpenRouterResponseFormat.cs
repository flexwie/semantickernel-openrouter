using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Helper class for creating response format configurations.
/// </summary>
public static class OpenRouterResponseFormat
{
    /// <summary>
    /// Standard text response format (default).
    /// </summary>
    public static object Text => new { type = "text" };

    /// <summary>
    /// JSON object response format. The model will generate valid JSON.
    /// </summary>
    public static object JsonObject => new { type = "json_object" };

    /// <summary>
    /// Creates a structured JSON response format with a specific schema.
    /// </summary>
    /// <param name="name">The name of the response format.</param>
    /// <param name="description">Description of the expected response.</param>
    /// <param name="schema">The JSON schema definition.</param>
    /// <returns>Response format configuration.</returns>
    public static object JsonSchema(string name, string? description = null, object? schema = null)
    {
        var format = new
        {
            type = "json_schema",
            json_schema = new
            {
                name = name,
                description = description,
                schema = schema,
                strict = true
            }
        };
        return format;
    }

    /// <summary>
    /// Creates a JSON response format with strict schema validation.
    /// </summary>
    /// <param name="name">The name of the response format.</param>
    /// <param name="schema">The JSON schema object.</param>
    /// <param name="strict">Whether to enforce strict schema compliance.</param>
    /// <returns>Response format configuration.</returns>
    public static object JsonSchemaStrict(string name, object schema, bool strict = true)
    {
        return new
        {
            type = "json_schema",
            json_schema = new
            {
                name = name,
                schema = schema,
                strict = strict
            }
        };
    }
}

/// <summary>
/// Represents a JSON schema for structured response formats.
/// </summary>
public sealed class JsonSchema
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchemaProperty> Properties { get; set; } = new();

    [JsonPropertyName("required")]
    public List<string> Required { get; set; } = new();

    [JsonPropertyName("additionalProperties")]
    public bool AdditionalProperties { get; set; } = false;
}

/// <summary>
/// Represents a property in a JSON schema.
/// </summary>
public sealed class JsonSchemaProperty
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("enum")]
    public List<string>? Enum { get; set; }

    [JsonPropertyName("items")]
    public JsonSchemaProperty? Items { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, JsonSchemaProperty>? Properties { get; set; }

    [JsonPropertyName("required")]
    public List<string>? Required { get; set; }
}