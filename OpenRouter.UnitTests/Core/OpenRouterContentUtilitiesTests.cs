using SemanticKernel.Connectors.OpenRouter.Core;
using SemanticKernel.Connectors.OpenRouter.Models;
using Xunit;

namespace OpenRouter.UnitTests.Core;

public class OpenRouterContentUtilitiesTests
{
    private readonly string _validBase64Image = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
    private readonly string _validBase64Pdf = "JVBERi0xLjQKJcfsj6IKMSAwIG9iago8PAovVHlwZSAvQ2F0YWxvZwo+PgplbmRvYmoKMiAwIG9iago8PAovVHlwZSAvUGFnZXMKL0tpZHMgWzMgMCBSXQovQ291bnQgMQo+PgplbmRvYmoKMyAwIG9iago8PAovVHlwZSAvUGFnZQovUGFyZW50IDIgMCBSCi9SZXNvdXJjZXMgPDwKL0ZvbnQgPDwKL0YxIDQgMCBSCj4+Cj4+Ci9NZWRpYUJveCBbMCAwIDYxMiA3OTJdCi9Db250ZW50cyA1IDAgUgo+PgplbmRvYmoKNCAwIG9iago8PAovVHlwZSAvRm9udAovU3VidHlwZSAvVHlwZTEKL0Jhc2VGb250IC9UaW1lcy1Sb21hbgo+PgplbmRvYmoKNSAwIG9iago8PAovTGVuZ3RoIDQ0Cj4+CnN0cmVhbQpCVAovRjEgMTIgVGYKNzIgNzIwIFRkCihIZWxsbyBXb3JsZCEpIFRqCkVUCmVuZHN0cmVhbQplbmRvYmoKeHJlZgowIDYKMDAwMDAwMDAwMCA2NTUzNSBmCjAwMDAwMDAwMDkgMDAwMDAgbgowMDAwMDAwMDc0IDAwMDAwIG4KMDAwMDAwMDEyMCAwMDAwMCBuCjAwMDAwMDAyNjUgMDAwMDAgbgowMDAwMDAwMzQ4IDAwMDAwIG4KQHR0YWlsZXIKPDwKL1NpemUgNgovUm9vdCAxIDAgUgo+PgpzdGFydHhyZWYKNDQxCiUlRU9G";

    [Fact]
    public void CreateImageFromBytes_WithValidPngData_CreatesImageContent()
    {
        // Arrange
        var imageBytes = Convert.FromBase64String(_validBase64Image);

        // Act
        var imageContent = OpenRouterContentUtilities.CreateImageFromBytes(imageBytes, "image/png", "high");

        // Assert
        Assert.IsType<OpenRouterImageContent>(imageContent);
        Assert.Equal("image_url", imageContent.Type);
        Assert.StartsWith("data:image/png;base64,", imageContent.ImageUrl.Url);
        Assert.Equal("high", imageContent.ImageUrl.Detail);
    }

    [Fact]
    public void CreateImageFromBytes_WithUnsupportedMimeType_ThrowsArgumentException()
    {
        // Arrange
        var imageBytes = Convert.FromBase64String(_validBase64Image);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            OpenRouterContentUtilities.CreateImageFromBytes(imageBytes, "image/gif"));

        Assert.Contains("Unsupported image format", exception.Message);
    }

    [Fact]
    public void CreateFileFromBytes_WithValidPdfData_CreatesFileContent()
    {
        // Arrange
        var fileBytes = Convert.FromBase64String(_validBase64Pdf);

        // Act
        var fileContent = OpenRouterContentUtilities.CreateFileFromBytes(fileBytes, "test.pdf", "application/pdf", "pdf-text");

        // Assert
        Assert.IsType<OpenRouterFileContent>(fileContent);
        Assert.Equal("file", fileContent.Type);
        Assert.Equal("test.pdf", fileContent.File.Filename);
        Assert.StartsWith("data:application/pdf;base64,", fileContent.File.FileData);
        Assert.Equal("pdf-text", fileContent.File.ProcessingEngine);
    }

    [Fact]
    public void CreateFileFromBytes_WithUnsupportedMimeType_ThrowsArgumentException()
    {
        // Arrange
        var fileBytes = Convert.FromBase64String(_validBase64Pdf);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            OpenRouterContentUtilities.CreateFileFromBytes(fileBytes, "test.exe", "application/exe"));

        Assert.Contains("Unsupported file format", exception.Message);
    }

    [Fact]
    public void IsValidDataUrl_WithValidDataUrl_ReturnsTrue()
    {
        // Arrange
        var validDataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";

        // Act
        var isValid = OpenRouterContentUtilities.IsValidDataUrl(validDataUrl);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidDataUrl_WithInvalidDataUrl_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(OpenRouterContentUtilities.IsValidDataUrl(""));
        Assert.False(OpenRouterContentUtilities.IsValidDataUrl("https://example.com/image.png"));
        Assert.False(OpenRouterContentUtilities.IsValidDataUrl("data:image/png,notbase64"));
        Assert.False(OpenRouterContentUtilities.IsValidDataUrl("image/png;base64,data"));
    }

    [Fact]
    public void IsValidHttpUrl_WithValidUrl_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(OpenRouterContentUtilities.IsValidHttpUrl("https://example.com/image.png"));
        Assert.True(OpenRouterContentUtilities.IsValidHttpUrl("http://example.com/image.png"));
    }

    [Fact]
    public void IsValidHttpUrl_WithInvalidUrl_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(OpenRouterContentUtilities.IsValidHttpUrl(""));
        Assert.False(OpenRouterContentUtilities.IsValidHttpUrl("ftp://example.com/image.png"));
        Assert.False(OpenRouterContentUtilities.IsValidHttpUrl("not-a-url"));
        Assert.False(OpenRouterContentUtilities.IsValidHttpUrl("file:///path/to/file"));
    }

    [Fact]
    public void GetMimeTypeFromExtension_ReturnsCorrectMimeType()
    {
        // Act & Assert
        Assert.Equal("image/png", OpenRouterContentUtilities.GetMimeTypeFromExtension("png"));
        Assert.Equal("image/png", OpenRouterContentUtilities.GetMimeTypeFromExtension(".png"));
        Assert.Equal("image/jpeg", OpenRouterContentUtilities.GetMimeTypeFromExtension("jpg"));
        Assert.Equal("image/jpeg", OpenRouterContentUtilities.GetMimeTypeFromExtension("jpeg"));
        Assert.Equal("image/webp", OpenRouterContentUtilities.GetMimeTypeFromExtension("webp"));
        Assert.Equal("application/pdf", OpenRouterContentUtilities.GetMimeTypeFromExtension("pdf"));
        Assert.Equal("text/plain", OpenRouterContentUtilities.GetMimeTypeFromExtension("txt"));
        Assert.Equal("application/octet-stream", OpenRouterContentUtilities.GetMimeTypeFromExtension("unknown"));
        Assert.Equal("application/octet-stream", OpenRouterContentUtilities.GetMimeTypeFromExtension(""));
    }

    [Fact]
    public void ExtractMimeTypeFromDataUrl_WithValidDataUrl_ReturnsMimeType()
    {
        // Arrange
        var dataUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";

        // Act
        var mimeType = OpenRouterContentUtilities.ExtractMimeTypeFromDataUrl(dataUrl);

        // Assert
        Assert.Equal("image/png", mimeType);
    }

    [Fact]
    public void ExtractMimeTypeFromDataUrl_WithInvalidDataUrl_ReturnsNull()
    {
        // Act & Assert
        Assert.Null(OpenRouterContentUtilities.ExtractMimeTypeFromDataUrl(""));
        Assert.Null(OpenRouterContentUtilities.ExtractMimeTypeFromDataUrl("https://example.com/image.png"));
        Assert.Null(OpenRouterContentUtilities.ExtractMimeTypeFromDataUrl("invalid"));
    }

    [Fact]
    public void ExtractBase64FromDataUrl_WithValidDataUrl_ReturnsBase64Data()
    {
        // Arrange
        var base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        var dataUrl = $"data:image/png;base64,{base64Data}";

        // Act
        var extractedData = OpenRouterContentUtilities.ExtractBase64FromDataUrl(dataUrl);

        // Assert
        Assert.Equal(base64Data, extractedData);
    }

    [Fact]
    public void ExtractBase64FromDataUrl_WithInvalidDataUrl_ReturnsNull()
    {
        // Act & Assert
        Assert.Null(OpenRouterContentUtilities.ExtractBase64FromDataUrl(""));
        Assert.Null(OpenRouterContentUtilities.ExtractBase64FromDataUrl("https://example.com/image.png"));
        Assert.Null(OpenRouterContentUtilities.ExtractBase64FromDataUrl("data:image/png,notbase64"));
    }

    [Fact]
    public void CreateDataUrl_CreatesValidDataUrl()
    {
        // Arrange
        var data = Convert.FromBase64String(_validBase64Image);

        // Act
        var dataUrl = OpenRouterContentUtilities.CreateDataUrl(data, "image/png");

        // Assert
        Assert.StartsWith("data:image/png;base64,", dataUrl);
        Assert.True(OpenRouterContentUtilities.IsValidDataUrl(dataUrl));
    }

    [Fact]
    public void SupportedImageMimeTypes_ContainsExpectedTypes()
    {
        // Assert
        Assert.Contains("image/png", OpenRouterContentUtilities.SupportedImageMimeTypes);
        Assert.Contains("image/jpeg", OpenRouterContentUtilities.SupportedImageMimeTypes);
        Assert.Contains("image/jpg", OpenRouterContentUtilities.SupportedImageMimeTypes);
        Assert.Contains("image/webp", OpenRouterContentUtilities.SupportedImageMimeTypes);
    }

    [Fact]
    public void SupportedFileMimeTypes_ContainsExpectedTypes()
    {
        // Assert
        Assert.Contains("application/pdf", OpenRouterContentUtilities.SupportedFileMimeTypes);
        Assert.Contains("text/plain", OpenRouterContentUtilities.SupportedFileMimeTypes);
        Assert.Contains("application/msword", OpenRouterContentUtilities.SupportedFileMimeTypes);
        Assert.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document", OpenRouterContentUtilities.SupportedFileMimeTypes);
    }

    [Fact]
    public void PdfProcessingEngines_HasExpectedValues()
    {
        // Assert
        Assert.Equal("pdf-text", OpenRouterContentUtilities.PdfProcessingEngines.PdfText);
        Assert.Equal("mistral-ocr", OpenRouterContentUtilities.PdfProcessingEngines.MistralOcr);
        Assert.Equal("native", OpenRouterContentUtilities.PdfProcessingEngines.Native);
    }
}