using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Custom JSON converter for OpenRouterMessage content that handles both string and array formats.
/// </summary>
public class OpenRouterMessageContentConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // For reading responses, we don't need complex handling - OpenRouter typically returns strings
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Create a new options instance without our custom converter to avoid infinite recursion
            var optionsForArray = new JsonSerializerOptions(options);
            optionsForArray.Converters.Clear();
            optionsForArray.PropertyNamingPolicy = options.PropertyNamingPolicy;
            optionsForArray.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
            
            // If we receive an array, deserialize as content items
            return JsonSerializer.Deserialize<OpenRouterContentItem[]>(ref reader, optionsForArray);
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string stringContent:
                writer.WriteStringValue(stringContent);
                break;
                
            case OpenRouterContentItem[] contentItems:
                // Create a new options instance without our custom converter to avoid infinite recursion
                var optionsForArray = new JsonSerializerOptions(options);
                optionsForArray.Converters.Clear();
                optionsForArray.PropertyNamingPolicy = options.PropertyNamingPolicy;
                optionsForArray.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
                JsonSerializer.Serialize(writer, contentItems, optionsForArray);
                break;
                
            case IEnumerable<OpenRouterContentItem> contentItems:
                var optionsForEnumerable = new JsonSerializerOptions(options);
                optionsForEnumerable.Converters.Clear();
                optionsForEnumerable.PropertyNamingPolicy = options.PropertyNamingPolicy;
                optionsForEnumerable.PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive;
                JsonSerializer.Serialize(writer, contentItems.ToArray(), optionsForEnumerable);
                break;
                
            default:
                writer.WriteNullValue();
                break;
        }
    }
}