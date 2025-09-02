using SemanticKernel.Connectors.OpenRouter.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterMessageContentConverterTests
{
    private readonly JsonSerializerOptions _options = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    [Fact]
    public void Write_WithStringContent_SerializesAsString()
    {
        // Arrange
        var message = new OpenRouterMessage
        {
            Role = "user",
            Content = "Hello, world!"
        };

        // Act
        var json = JsonSerializer.Serialize(message, _options);

        // Assert
        Assert.Contains("\"content\":\"Hello, world!\"", json);
    }

    [Fact]
    public void Write_WithContentArray_SerializesAsArray()
    {
        // Arrange
        var contentItems = new OpenRouterContentItem[]
        {
            OpenRouterContentHelper.CreateText("What's in this image?"),
            OpenRouterContentHelper.CreateImageUrl("https://example.com/image.jpg")
        };

        var message = new OpenRouterMessage
        {
            Role = "user",
            Content = contentItems
        };

        // Act
        var json = JsonSerializer.Serialize(message, _options);

        // Assert
        Assert.Contains("\"content\":[", json);
        Assert.Contains("\"type\":\"text\"", json);
        Assert.Contains("\"type\":\"image_url\"", json);
        Assert.Contains("\"text\":\"What\\u0027s in this image?\"", json);
        Assert.Contains("\"url\":\"https://example.com/image.jpg\"", json);
    }

    [Fact]
    public void Write_WithNullContent_SerializesAsNull()
    {
        // Arrange
        var message = new OpenRouterMessage
        {
            Role = "user",
            Content = null
        };

        // Act
        var json = JsonSerializer.Serialize(message, _options);

        // Assert
        Assert.Contains("\"content\":null", json);
    }

    [Fact]
    public void Read_WithStringContent_DeserializesAsString()
    {
        // Arrange
        var json = """{"role":"user","content":"Hello, world!"}""";

        // Act
        var message = JsonSerializer.Deserialize<OpenRouterMessage>(json, _options);

        // Assert
        Assert.NotNull(message);
        Assert.Equal("Hello, world!", message.Content);
        Assert.True(message.IsTextOnly);
        Assert.False(message.IsMultimodal);
        Assert.Equal("Hello, world!", message.TextContent);
    }

    [Fact]
    public void Read_WithContentArray_DeserializesAsArray()
    {
        // Arrange
        var json = """
        {
            "role": "user",
            "content": [
                {"type": "text", "text": "What's in this image?"},
                {"type": "image_url", "image_url": {"url": "https://example.com/image.jpg"}}
            ]
        }
        """;

        // Act
        var message = JsonSerializer.Deserialize<OpenRouterMessage>(json, _options);

        // Assert
        Assert.NotNull(message);
        Assert.IsType<OpenRouterContentItem[]>(message.Content);
        Assert.True(message.IsMultimodal);
        Assert.False(message.IsTextOnly);
        
        var contentItems = message.ContentItems;
        Assert.NotNull(contentItems);
        Assert.Equal(2, contentItems.Length);
        
        Assert.IsType<OpenRouterTextContent>(contentItems[0]);
        Assert.Equal("What's in this image?", ((OpenRouterTextContent)contentItems[0]).Text);
        
        Assert.IsType<OpenRouterImageContent>(contentItems[1]);
        Assert.Equal("https://example.com/image.jpg", ((OpenRouterImageContent)contentItems[1]).ImageUrl.Url);
    }

    [Fact]
    public void Read_WithNullContent_DeserializesAsNull()
    {
        // Arrange
        var json = """{"role":"user","content":null}""";

        // Act
        var message = JsonSerializer.Deserialize<OpenRouterMessage>(json, _options);

        // Assert
        Assert.NotNull(message);
        Assert.Null(message.Content);
        Assert.False(message.IsTextOnly);
        Assert.False(message.IsMultimodal);
    }

    [Fact]
    public void TextContent_Property_WorksWithStringContent()
    {
        // Arrange
        var message = new OpenRouterMessage
        {
            Role = "user"
        };

        // Act
        message.TextContent = "Test message";

        // Assert
        Assert.Equal("Test message", message.Content);
        Assert.Equal("Test message", message.TextContent);
        Assert.True(message.IsTextOnly);
    }

    [Fact]
    public void ContentItems_Property_WorksWithContentArray()
    {
        // Arrange
        var message = new OpenRouterMessage
        {
            Role = "user"
        };

        var contentItems = new OpenRouterContentItem[]
        {
            OpenRouterContentHelper.CreateText("Hello"),
            OpenRouterContentHelper.CreateImageUrl("https://example.com/image.jpg")
        };

        // Act
        message.ContentItems = contentItems;

        // Assert
        Assert.Equal(contentItems, message.Content);
        Assert.Equal(contentItems, message.ContentItems);
        Assert.True(message.IsMultimodal);
    }

    [Fact]
    public void ComplexMessage_WithMultipleContentTypes_SerializesAndDeserializesCorrectly()
    {
        // Arrange
        var originalMessage = new OpenRouterMessage
        {
            Role = "user",
            Name = "testuser",
            Content = new OpenRouterContentItem[]
            {
                OpenRouterContentHelper.CreateText("Analyze this document and image:"),
                OpenRouterContentHelper.CreateFile("document.pdf", "JVBERi0xLjQ=", "application/pdf", "pdf-text"),
                OpenRouterContentHelper.CreateImageUrl("https://example.com/chart.png", "high")
            }
        };

        // Act
        var json = JsonSerializer.Serialize(originalMessage, _options);
        var deserializedMessage = JsonSerializer.Deserialize<OpenRouterMessage>(json, _options);

        // Assert
        Assert.NotNull(deserializedMessage);
        Assert.Equal("user", deserializedMessage.Role);
        Assert.Equal("testuser", deserializedMessage.Name);
        Assert.True(deserializedMessage.IsMultimodal);
        
        var contentItems = deserializedMessage.ContentItems;
        Assert.NotNull(contentItems);
        Assert.Equal(3, contentItems.Length);
        
        // Text content
        Assert.IsType<OpenRouterTextContent>(contentItems[0]);
        Assert.Equal("Analyze this document and image:", ((OpenRouterTextContent)contentItems[0]).Text);
        
        // File content
        Assert.IsType<OpenRouterFileContent>(contentItems[1]);
        var fileContent = (OpenRouterFileContent)contentItems[1];
        Assert.Equal("document.pdf", fileContent.File.Filename);
        Assert.Equal("pdf-text", fileContent.File.ProcessingEngine);
        
        // Image content
        Assert.IsType<OpenRouterImageContent>(contentItems[2]);
        var imageContent = (OpenRouterImageContent)contentItems[2];
        Assert.Equal("https://example.com/chart.png", imageContent.ImageUrl.Url);
        Assert.Equal("high", imageContent.ImageUrl.Detail);
    }
}