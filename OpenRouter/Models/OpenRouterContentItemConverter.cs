using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Custom JSON converter for OpenRouterContentItem polymorphic serialization.
/// </summary>
public class OpenRouterContentItemConverter : JsonConverter<OpenRouterContentItem>
{
    public override OpenRouterContentItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var rootElement = document.RootElement;

        if (!rootElement.TryGetProperty("type", out var typeProperty))
        {
            throw new JsonException("Missing 'type' property in content item");
        }

        var typeValue = typeProperty.GetString();

        // Create appropriate type based on discriminator
        return typeValue switch
        {
            "text" => JsonSerializer.Deserialize<OpenRouterTextContent>(rootElement.GetRawText(), options),
            "image_url" => JsonSerializer.Deserialize<OpenRouterImageContent>(rootElement.GetRawText(), options),
            "file" => JsonSerializer.Deserialize<OpenRouterFileContent>(rootElement.GetRawText(), options),
            _ => throw new JsonException($"Unknown content item type: {typeValue}")
        };
    }

    public override void Write(Utf8JsonWriter writer, OpenRouterContentItem value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}