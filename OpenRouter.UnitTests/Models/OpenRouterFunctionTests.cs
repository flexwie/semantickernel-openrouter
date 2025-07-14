using SemanticKernel.Connectors.OpenRouter.Models;
using System.Text.Json;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterFunctionTests
{
    [Fact]
    public void OpenRouterFunction_SerializesCorrectly()
    {
        // Arrange
        var function = new OpenRouterFunction
        {
            Name = "test_function",
            Description = "A test function",
            Parameters = new OpenRouterFunctionParameters
            {
                Type = "object",
                Properties = new Dictionary<string, OpenRouterFunctionProperty>
                {
                    ["input"] = new OpenRouterFunctionProperty
                    {
                        Type = "string",
                        Description = "Input parameter"
                    }
                },
                Required = ["input"]
            }
        };

        // Act
        var json = JsonSerializer.Serialize(function);
        var deserialized = JsonSerializer.Deserialize<OpenRouterFunction>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("test_function", deserialized.Name);
        Assert.Equal("A test function", deserialized.Description);
        Assert.NotNull(deserialized.Parameters);
        Assert.Equal("object", deserialized.Parameters.Type);
        Assert.Contains("input", deserialized.Parameters.Properties.Keys);
        Assert.Single(deserialized.Parameters.Required);
        Assert.Contains("input", deserialized.Parameters.Required);
    }

    [Fact]
    public void OpenRouterToolCall_SerializesCorrectly()
    {
        // Arrange
        var toolCall = new OpenRouterToolCall
        {
            Id = "call_123",
            Type = "function",
            Function = new OpenRouterFunctionCall
            {
                Name = "test_function",
                Arguments = "{\"input\":\"test\"}"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(toolCall);
        var deserialized = JsonSerializer.Deserialize<OpenRouterToolCall>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("call_123", deserialized.Id);
        Assert.Equal("function", deserialized.Type);
        Assert.NotNull(deserialized.Function);
        Assert.Equal("test_function", deserialized.Function.Name);
        Assert.Equal("{\"input\":\"test\"}", deserialized.Function.Arguments);
    }

    [Fact]
    public void OpenRouterTool_SerializesCorrectly()
    {
        // Arrange
        var tool = new OpenRouterTool
        {
            Type = "function",
            Function = new OpenRouterFunction
            {
                Name = "test_function",
                Description = "A test function"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(tool);
        var deserialized = JsonSerializer.Deserialize<OpenRouterTool>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("function", deserialized.Type);
        Assert.NotNull(deserialized.Function);
        Assert.Equal("test_function", deserialized.Function.Name);
        Assert.Equal("A test function", deserialized.Function.Description);
    }

    [Fact]
    public void OpenRouterFunctionProperty_WithEnum_SerializesCorrectly()
    {
        // Arrange
        var property = new OpenRouterFunctionProperty
        {
            Type = "string",
            Description = "Color selection",
            Enum = ["red", "green", "blue"]
        };

        // Act
        var json = JsonSerializer.Serialize(property);
        var deserialized = JsonSerializer.Deserialize<OpenRouterFunctionProperty>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("string", deserialized.Type);
        Assert.Equal("Color selection", deserialized.Description);
        Assert.NotNull(deserialized.Enum);
        Assert.Equal(3, deserialized.Enum.Count);
        Assert.Contains("red", deserialized.Enum);
        Assert.Contains("green", deserialized.Enum);
        Assert.Contains("blue", deserialized.Enum);
    }

    [Fact]
    public void OpenRouterFunctionProperty_WithArrayType_SerializesCorrectly()
    {
        // Arrange
        var property = new OpenRouterFunctionProperty
        {
            Type = "array",
            Description = "List of items",
            Items = new OpenRouterFunctionProperty
            {
                Type = "string"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(property);
        var deserialized = JsonSerializer.Deserialize<OpenRouterFunctionProperty>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("array", deserialized.Type);
        Assert.Equal("List of items", deserialized.Description);
        Assert.NotNull(deserialized.Items);
        Assert.Equal("string", deserialized.Items.Type);
    }
}