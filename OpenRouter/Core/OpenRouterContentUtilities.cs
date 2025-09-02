using SemanticKernel.Connectors.OpenRouter.Models;
using System.Text;

namespace SemanticKernel.Connectors.OpenRouter.Core;

/// <summary>
/// Utilities for working with OpenRouter multimodal content.
/// </summary>
public static class OpenRouterContentUtilities
{
    /// <summary>
    /// Supported image MIME types.
    /// </summary>
    public static readonly HashSet<string> SupportedImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/jpg", 
        "image/webp"
    };

    /// <summary>
    /// Supported file MIME types.
    /// </summary>
    public static readonly HashSet<string> SupportedFileMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "text/plain",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    /// <summary>
    /// Available PDF processing engines.
    /// </summary>
    public static class PdfProcessingEngines
    {
        /// <summary>
        /// Free text extraction, best for clear text documents.
        /// </summary>
        public const string PdfText = "pdf-text";

        /// <summary>
        /// OCR processing at $2 per 1,000 pages, best for scanned documents.
        /// </summary>
        public const string MistralOcr = "mistral-ocr";

        /// <summary>
        /// Model-specific file processing.
        /// </summary>
        public const string Native = "native";
    }

    /// <summary>
    /// Creates an image content item from a file path.
    /// </summary>
    /// <param name="filePath">The path to the image file.</param>
    /// <param name="detail">The detail level for image processing.</param>
    /// <returns>An image content item.</returns>
    /// <exception cref="ArgumentException">Thrown when the file type is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static OpenRouterImageContent CreateImageFromFile(string filePath, string? detail = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Image file not found: {filePath}");
        }

        var mimeType = GetMimeTypeFromExtension(Path.GetExtension(filePath));
        if (!SupportedImageMimeTypes.Contains(mimeType))
        {
            throw new ArgumentException($"Unsupported image format: {mimeType}. Supported formats: {string.Join(", ", SupportedImageMimeTypes)}");
        }

        var fileBytes = File.ReadAllBytes(filePath);
        var base64Data = Convert.ToBase64String(fileBytes);

        return OpenRouterContentHelper.CreateImageBase64(base64Data, mimeType, detail);
    }

    /// <summary>
    /// Creates an image content item from byte data.
    /// </summary>
    /// <param name="imageData">The image data.</param>
    /// <param name="mimeType">The MIME type of the image.</param>
    /// <param name="detail">The detail level for image processing.</param>
    /// <returns>An image content item.</returns>
    /// <exception cref="ArgumentException">Thrown when the MIME type is not supported.</exception>
    public static OpenRouterImageContent CreateImageFromBytes(byte[] imageData, string mimeType, string? detail = null)
    {
        if (!SupportedImageMimeTypes.Contains(mimeType))
        {
            throw new ArgumentException($"Unsupported image format: {mimeType}. Supported formats: {string.Join(", ", SupportedImageMimeTypes)}");
        }

        var base64Data = Convert.ToBase64String(imageData);
        return OpenRouterContentHelper.CreateImageBase64(base64Data, mimeType, detail);
    }

    /// <summary>
    /// Creates a file content item from a file path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="processingEngine">The processing engine to use for PDFs.</param>
    /// <returns>A file content item.</returns>
    /// <exception cref="ArgumentException">Thrown when the file type is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    public static OpenRouterFileContent CreateFileFromPath(string filePath, string? processingEngine = null)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var mimeType = GetMimeTypeFromExtension(Path.GetExtension(filePath));
        if (!SupportedFileMimeTypes.Contains(mimeType))
        {
            throw new ArgumentException($"Unsupported file format: {mimeType}. Supported formats: {string.Join(", ", SupportedFileMimeTypes)}");
        }

        var fileBytes = File.ReadAllBytes(filePath);
        var base64Data = Convert.ToBase64String(fileBytes);
        var filename = Path.GetFileName(filePath);

        return OpenRouterContentHelper.CreateFile(filename, base64Data, mimeType, processingEngine);
    }

    /// <summary>
    /// Creates a file content item from byte data.
    /// </summary>
    /// <param name="fileData">The file data.</param>
    /// <param name="filename">The filename.</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    /// <param name="processingEngine">The processing engine to use for PDFs.</param>
    /// <returns>A file content item.</returns>
    /// <exception cref="ArgumentException">Thrown when the MIME type is not supported.</exception>
    public static OpenRouterFileContent CreateFileFromBytes(byte[] fileData, string filename, string mimeType, string? processingEngine = null)
    {
        if (!SupportedFileMimeTypes.Contains(mimeType))
        {
            throw new ArgumentException($"Unsupported file format: {mimeType}. Supported formats: {string.Join(", ", SupportedFileMimeTypes)}");
        }

        var base64Data = Convert.ToBase64String(fileData);
        return OpenRouterContentHelper.CreateFile(filename, base64Data, mimeType, processingEngine);
    }

    /// <summary>
    /// Validates that a data URL has the correct format.
    /// </summary>
    /// <param name="dataUrl">The data URL to validate.</param>
    /// <returns>True if the data URL is valid, false otherwise.</returns>
    public static bool IsValidDataUrl(string dataUrl)
    {
        if (string.IsNullOrWhiteSpace(dataUrl))
        {
            return false;
        }

        return dataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && 
               dataUrl.Contains(";base64,", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates that a URL is a valid HTTP/HTTPS URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid, false otherwise.</returns>
    public static bool IsValidHttpUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Gets the MIME type from a file extension.
    /// </summary>
    /// <param name="extension">The file extension (with or without leading dot).</param>
    /// <returns>The MIME type.</returns>
    public static string GetMimeTypeFromExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return "application/octet-stream";
        }

        extension = extension.TrimStart('.').ToLowerInvariant();

        return extension switch
        {
            "png" => "image/png",
            "jpg" or "jpeg" => "image/jpeg",
            "webp" => "image/webp",
            "pdf" => "application/pdf",
            "txt" => "text/plain",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    /// <summary>
    /// Extracts the MIME type from a data URL.
    /// </summary>
    /// <param name="dataUrl">The data URL.</param>
    /// <returns>The MIME type, or null if the data URL is invalid.</returns>
    public static string? ExtractMimeTypeFromDataUrl(string dataUrl)
    {
        if (!IsValidDataUrl(dataUrl))
        {
            return null;
        }

        var parts = dataUrl.Split(';');
        if (parts.Length < 2)
        {
            return null;
        }

        return parts[0].Substring(5); // Remove "data:" prefix
    }

    /// <summary>
    /// Extracts the base64 data from a data URL.
    /// </summary>
    /// <param name="dataUrl">The data URL.</param>
    /// <returns>The base64 data, or null if the data URL is invalid.</returns>
    public static string? ExtractBase64FromDataUrl(string dataUrl)
    {
        if (!IsValidDataUrl(dataUrl))
        {
            return null;
        }

        var base64Index = dataUrl.IndexOf(";base64,", StringComparison.OrdinalIgnoreCase);
        if (base64Index == -1)
        {
            return null;
        }

        return dataUrl.Substring(base64Index + 8); // Remove ";base64," prefix
    }

    /// <summary>
    /// Converts byte data to a data URL.
    /// </summary>
    /// <param name="data">The byte data.</param>
    /// <param name="mimeType">The MIME type.</param>
    /// <returns>The data URL.</returns>
    public static string CreateDataUrl(byte[] data, string mimeType)
    {
        var base64Data = Convert.ToBase64String(data);
        return $"data:{mimeType};base64,{base64Data}";
    }
}