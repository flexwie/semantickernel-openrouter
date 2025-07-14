using SemanticKernel.Connectors.OpenRouter.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterEnhancedConfigurationTests
{
    [Fact]
    public void OpenRouterExecutionSettings_WithEnhancedOptions_SerializesCorrectly()
    {
        // Arrange
        var settings = new OpenRouterExecutionSettings
        {
            ModelId = "anthropic/claude-3-haiku",
            Temperature = 0.7,
            MaxTokens = 1000,
            Seed = 12345,
            ResponseFormat = OpenRouterResponseFormat.JsonObject,
            LogitBias = OpenRouterLogitBias.Create((50256, -50), (198, 25)),
            User = "test-user-123",
            MaxCompletionTokens = 800,
            Store = true,
            Metadata = new Dictionary<string, object> { ["session"] = "test-session" },
            TopLogprobs = 5,
            LogProbs = true,
            ServiceTier = "auto",
            ParallelToolCalls = true
        };

        // Act & Assert - Should not throw
        var json = JsonSerializer.Serialize(settings);
        Assert.NotNull(json);
        Assert.Contains("\"seed\":12345", json);
        Assert.Contains("\"user\":\"test-user-123\"", json);
        Assert.Contains("\"store\":true", json);
    }

    [Fact]
    public void OpenRouterResponseFormat_Text_CreatesCorrectFormat()
    {
        // Act
        var format = OpenRouterResponseFormat.Text;

        // Assert
        var json = JsonSerializer.Serialize(format);
        Assert.Contains("\"type\":\"text\"", json);
    }

    [Fact]
    public void OpenRouterResponseFormat_JsonObject_CreatesCorrectFormat()
    {
        // Act
        var format = OpenRouterResponseFormat.JsonObject;

        // Assert
        var json = JsonSerializer.Serialize(format);
        Assert.Contains("\"type\":\"json_object\"", json);
    }

    [Fact]
    public void OpenRouterResponseFormat_JsonSchema_CreatesCorrectFormat()
    {
        // Arrange
        var schema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchemaProperty>
            {
                ["name"] = new JsonSchemaProperty { Type = "string", Description = "Person's name" },
                ["age"] = new JsonSchemaProperty { Type = "integer", Description = "Person's age" }
            },
            Required = ["name"]
        };

        // Act
        var format = OpenRouterResponseFormat.JsonSchema("person", "A person object", schema);

        // Assert
        var json = JsonSerializer.Serialize(format);
        Assert.Contains("\"type\":\"json_schema\"", json);
        Assert.Contains("\"name\":\"person\"", json);
        Assert.Contains("\"strict\":true", json);
    }

    [Fact]
    public void OpenRouterLogitBias_Create_WithValidBiases_CreatesCorrectDictionary()
    {
        // Act
        var bias = OpenRouterLogitBias.Create((100, 50), (200, -25), (300, 0));

        // Assert
        Assert.Equal(3, bias.Count);
        Assert.Equal(50, bias[100]);
        Assert.Equal(-25, bias[200]);
        Assert.Equal(0, bias[300]);
    }

    [Fact]
    public void OpenRouterLogitBias_Create_WithInvalidBias_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OpenRouterLogitBias.Create((100, 150)));
        Assert.Throws<ArgumentException>(() => OpenRouterLogitBias.Create((100, -150)));
    }

    [Fact]
    public void OpenRouterLogitBias_Suppress_CreatesMaxNegativeBias()
    {
        // Act
        var bias = OpenRouterLogitBias.Suppress(100, 200, 300);

        // Assert
        Assert.Equal(3, bias.Count);
        Assert.All(bias.Values, value => Assert.Equal(-100, value));
    }

    [Fact]
    public void OpenRouterLogitBias_Encourage_CreatesMaxPositiveBias()
    {
        // Act
        var bias = OpenRouterLogitBias.Encourage(100, 200, 300);

        // Assert
        Assert.Equal(3, bias.Count);
        Assert.All(bias.Values, value => Assert.Equal(100, value));
    }

    [Fact]
    public void OpenRouterLogitBias_Discourage_WithValidBias_CreatesCorrectDictionary()
    {
        // Act
        var bias = OpenRouterLogitBias.Discourage(-50, 100, 200);

        // Assert
        Assert.Equal(2, bias.Count);
        Assert.All(bias.Values, value => Assert.Equal(-50, value));
    }

    [Fact]
    public void OpenRouterLogitBias_Discourage_WithPositiveBias_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OpenRouterLogitBias.Discourage(50, 100));
    }

    [Fact]
    public void OpenRouterLogitBias_Favor_WithValidBias_CreatesCorrectDictionary()
    {
        // Act
        var bias = OpenRouterLogitBias.Favor(30, 100, 200);

        // Assert
        Assert.Equal(2, bias.Count);
        Assert.All(bias.Values, value => Assert.Equal(30, value));
    }

    [Fact]
    public void OpenRouterLogitBias_Favor_WithNegativeBias_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => OpenRouterLogitBias.Favor(-30, 100));
    }

    [Fact]
    public void OpenRouterLogitBias_Merge_CombinesMultipleDictionaries()
    {
        // Arrange
        var bias1 = OpenRouterLogitBias.Create((100, 50));
        var bias2 = OpenRouterLogitBias.Create((200, -25));
        var bias3 = OpenRouterLogitBias.Create((100, 75)); // Override token 100

        // Act
        var merged = OpenRouterLogitBias.Merge(bias1, bias2, bias3);

        // Assert
        Assert.Equal(2, merged.Count);
        Assert.Equal(75, merged[100]); // Should use the last value
        Assert.Equal(-25, merged[200]);
    }

    [Fact]
    public void JsonSchema_SerializesCorrectly()
    {
        // Arrange
        var schema = new JsonSchema
        {
            Type = "object",
            Properties = new Dictionary<string, JsonSchemaProperty>
            {
                ["name"] = new JsonSchemaProperty { Type = "string", Description = "The name" },
                ["items"] = new JsonSchemaProperty 
                { 
                    Type = "array", 
                    Items = new JsonSchemaProperty { Type = "string" }
                }
            },
            Required = ["name"],
            AdditionalProperties = false
        };

        // Act
        var json = JsonSerializer.Serialize(schema);

        // Assert
        Assert.Contains("\"type\":\"object\"", json);
        Assert.Contains("\"required\":[\"name\"]", json);
        Assert.Contains("\"additionalProperties\":false", json);
    }
}