using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

/// <summary>
/// Base class for OpenRouter content items in messages.
/// </summary>
[JsonConverter(typeof(OpenRouterContentItemConverter))]
public abstract class OpenRouterContentItem
{
    /// <summary>
    /// The type of content item.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}

/// <summary>
/// Text content item for OpenRouter messages.
/// </summary>
public sealed class OpenRouterTextContent : OpenRouterContentItem
{
    /// <inheritdoc />
    public override string Type => "text";

    /// <summary>
    /// The text content.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}

/// <summary>
/// Image content item for OpenRouter messages.
/// </summary>
public sealed class OpenRouterImageContent : OpenRouterContentItem
{
    /// <inheritdoc />
    public override string Type => "image_url";

    /// <summary>
    /// The image URL configuration.
    /// </summary>
    [JsonPropertyName("image_url")]
    public required OpenRouterImageUrl ImageUrl { get; set; }
}

/// <summary>
/// Image URL configuration for OpenRouter image content.
/// </summary>
public sealed class OpenRouterImageUrl
{
    /// <summary>
    /// The URL or base64 data URL of the image.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// The detail level for image processing. Can be "auto", "low", or "high".
    /// </summary>
    [JsonPropertyName("detail")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Detail { get; set; }
}

/// <summary>
/// File content item for OpenRouter messages (PDFs, documents).
/// </summary>
public sealed class OpenRouterFileContent : OpenRouterContentItem
{
    /// <inheritdoc />
    public override string Type => "file";

    /// <summary>
    /// The file configuration.
    /// </summary>
    [JsonPropertyName("file")]
    public required OpenRouterFile File { get; set; }
}

/// <summary>
/// File configuration for OpenRouter file content.
/// </summary>
public sealed class OpenRouterFile
{
    /// <summary>
    /// The filename of the uploaded file.
    /// </summary>
    [JsonPropertyName("filename")]
    public required string Filename { get; set; }

    /// <summary>
    /// The base64-encoded file data with data URL prefix (e.g., "data:application/pdf;base64,...").
    /// </summary>
    [JsonPropertyName("file_data")]
    public required string FileData { get; set; }

    /// <summary>
    /// The processing engine to use for file processing.
    /// Options: "pdf-text" (free, best for clear text), "mistral-ocr" ($2/1000 pages), "native" (model-specific).
    /// </summary>
    [JsonPropertyName("processing_engine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ProcessingEngine { get; set; }
}

/// <summary>
/// Helper class for creating OpenRouter content items.
/// </summary>
public static class OpenRouterContentHelper
{
    /// <summary>
    /// Creates a text content item.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <returns>A text content item.</returns>
    public static OpenRouterTextContent CreateText(string text)
    {
        return new OpenRouterTextContent { Text = text };
    }

    /// <summary>
    /// Creates an image content item from a URL.
    /// </summary>
    /// <param name="url">The image URL.</param>
    /// <param name="detail">The detail level for image processing.</param>
    /// <returns>An image content item.</returns>
    public static OpenRouterImageContent CreateImageUrl(string url, string? detail = null)
    {
        return new OpenRouterImageContent
        {
            ImageUrl = new OpenRouterImageUrl { Url = url, Detail = detail }
        };
    }

    /// <summary>
    /// Creates an image content item from base64 data.
    /// </summary>
    /// <param name="base64Data">The base64-encoded image data.</param>
    /// <param name="mimeType">The MIME type of the image (e.g., "image/png", "image/jpeg").</param>
    /// <param name="detail">The detail level for image processing.</param>
    /// <returns>An image content item.</returns>
    public static OpenRouterImageContent CreateImageBase64(string base64Data, string mimeType, string? detail = null)
    {
        var dataUrl = $"data:{mimeType};base64,{base64Data}";
        return new OpenRouterImageContent
        {
            ImageUrl = new OpenRouterImageUrl { Url = dataUrl, Detail = detail }
        };
    }

    /// <summary>
    /// Creates a file content item from base64 data.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="base64Data">The base64-encoded file data.</param>
    /// <param name="mimeType">The MIME type of the file (e.g., "application/pdf").</param>
    /// <param name="processingEngine">The processing engine to use.</param>
    /// <returns>A file content item.</returns>
    public static OpenRouterFileContent CreateFile(string filename, string base64Data, string mimeType, string? processingEngine = null)
    {
        var dataUrl = $"data:{mimeType};base64,{base64Data}";
        return new OpenRouterFileContent
        {
            File = new OpenRouterFile 
            { 
                Filename = filename, 
                FileData = dataUrl,
                ProcessingEngine = processingEngine
            }
        };
    }
}