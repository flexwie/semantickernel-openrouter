using SemanticKernel.Connectors.OpenRouter.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterContentTests
{
    [Fact]
    public void OpenRouterTextContent_SerializesCorrectly()
    {
        // Arrange
        var textContent = new OpenRouterTextContent { Text = "Hello, world!" };

        // Act
        var json = JsonSerializer.Serialize(textContent);

        // Assert
        Assert.Contains("\"type\":\"text\"", json);
        Assert.Contains("\"text\":\"Hello, world!\"", json);
    }

    [Fact]
    public void OpenRouterImageContent_SerializesCorrectly()
    {
        // Arrange
        var imageContent = new OpenRouterImageContent
        {
            ImageUrl = new OpenRouterImageUrl
            {
                Url = "https://example.com/image.jpg",
                Detail = "high"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(imageContent);

        // Assert
        Assert.Contains("\"type\":\"image_url\"", json);
        Assert.Contains("\"url\":\"https://example.com/image.jpg\"", json);
        Assert.Contains("\"detail\":\"high\"", json);
    }

    [Fact]
    public void OpenRouterFileContent_SerializesCorrectly()
    {
        // Arrange
        var fileContent = new OpenRouterFileContent
        {
            File = new OpenRouterFile
            {
                Filename = "document.pdf",
                FileData = "data:application/pdf;base64,JVBERi0xLjQ=",
                ProcessingEngine = "pdf-text"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(fileContent);

        // Assert
        Assert.Contains("\"type\":\"file\"", json);
        Assert.Contains("\"filename\":\"document.pdf\"", json);
        Assert.Contains("\"file_data\":\"data:application/pdf;base64,JVBERi0xLjQ=\"", json);
        Assert.Contains("\"processing_engine\":\"pdf-text\"", json);
    }

    [Fact]
    public void OpenRouterContentItem_DeserializesPolymorphically()
    {
        // Arrange
        var json = """
        [
            {"type": "text", "text": "Hello"},
            {"type": "image_url", "image_url": {"url": "https://example.com/image.jpg"}},
            {"type": "file", "file": {"filename": "doc.pdf", "file_data": "data:application/pdf;base64,JVBERi0xLjQ="}}
        ]
        """;

        // Act
        var contentItems = JsonSerializer.Deserialize<OpenRouterContentItem[]>(json);

        // Assert
        Assert.NotNull(contentItems);
        Assert.Equal(3, contentItems.Length);
        
        Assert.IsType<OpenRouterTextContent>(contentItems[0]);
        Assert.Equal("Hello", ((OpenRouterTextContent)contentItems[0]).Text);
        
        Assert.IsType<OpenRouterImageContent>(contentItems[1]);
        Assert.Equal("https://example.com/image.jpg", ((OpenRouterImageContent)contentItems[1]).ImageUrl.Url);
        
        Assert.IsType<OpenRouterFileContent>(contentItems[2]);
        Assert.Equal("doc.pdf", ((OpenRouterFileContent)contentItems[2]).File.Filename);
    }

    [Fact]
    public void OpenRouterContentHelper_CreateText_Works()
    {
        // Act
        var content = OpenRouterContentHelper.CreateText("Test message");

        // Assert
        Assert.IsType<OpenRouterTextContent>(content);
        Assert.Equal("text", content.Type);
        Assert.Equal("Test message", content.Text);
    }

    [Fact]
    public void OpenRouterContentHelper_CreateImageUrl_Works()
    {
        // Act
        var content = OpenRouterContentHelper.CreateImageUrl("https://example.com/image.png", "high");

        // Assert
        Assert.IsType<OpenRouterImageContent>(content);
        Assert.Equal("image_url", content.Type);
        Assert.Equal("https://example.com/image.png", content.ImageUrl.Url);
        Assert.Equal("high", content.ImageUrl.Detail);
    }

    [Fact]
    public void OpenRouterContentHelper_CreateImageBase64_Works()
    {
        // Arrange
        var base64Data = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";

        // Act
        var content = OpenRouterContentHelper.CreateImageBase64(base64Data, "image/png", "low");

        // Assert
        Assert.IsType<OpenRouterImageContent>(content);
        Assert.Equal("image_url", content.Type);
        Assert.StartsWith("data:image/png;base64,", content.ImageUrl.Url);
        Assert.Equal("low", content.ImageUrl.Detail);
    }

    [Fact]
    public void OpenRouterContentHelper_CreateFile_Works()
    {
        // Arrange
        var base64Data = "JVBERi0xLjQ=";

        // Act
        var content = OpenRouterContentHelper.CreateFile("test.pdf", base64Data, "application/pdf", "mistral-ocr");

        // Assert
        Assert.IsType<OpenRouterFileContent>(content);
        Assert.Equal("file", content.Type);
        Assert.Equal("test.pdf", content.File.Filename);
        Assert.StartsWith("data:application/pdf;base64,", content.File.FileData);
        Assert.Equal("mistral-ocr", content.File.ProcessingEngine);
    }

    [Fact]
    public void OpenRouterImageUrl_CanHaveNullDetail()
    {
        // Arrange
        var imageUrl = new OpenRouterImageUrl
        {
            Url = "https://example.com/image.jpg"
        };

        // Act
        var json = JsonSerializer.Serialize(imageUrl);

        // Assert
        Assert.Contains("\"url\":\"https://example.com/image.jpg\"", json);
        Assert.DoesNotContain("\"detail\"", json);
    }

    [Fact]
    public void OpenRouterFile_CanHaveNullProcessingEngine()
    {
        // Arrange
        var file = new OpenRouterFile
        {
            Filename = "document.pdf",
            FileData = "data:application/pdf;base64,JVBERi0xLjQ="
        };

        // Act
        var json = JsonSerializer.Serialize(file);

        // Assert
        Assert.Contains("\"filename\":\"document.pdf\"", json);
        Assert.DoesNotContain("\"processing_engine\"", json);
    }
}