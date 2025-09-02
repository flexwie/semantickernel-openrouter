using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernel.Connectors.OpenRouter.Models;
using SemanticKernel.Connectors.OpenRouter.Services;
using System.Reflection;
using Xunit;

namespace OpenRouter.UnitTests.Services;

public class OpenRouterMultimodalTests
{
    [Fact]
    public void CreateOpenRouterMessage_WithSimpleTextMessage_CreatesStringContent()
    {
        // Arrange
        var chatMessage = new ChatMessageContent(AuthorRole.User, "Hello, world!");

        // Act
        var openRouterMessage = CallCreateOpenRouterMessage(chatMessage);

        // Assert
        Assert.Equal("user", openRouterMessage.Role);
        Assert.Equal("Hello, world!", openRouterMessage.Content);
        Assert.True(openRouterMessage.IsTextOnly);
        Assert.False(openRouterMessage.IsMultimodal);
    }

    [Fact]
    public void CreateOpenRouterMessage_WithEmptyMessage_CreatesNullContent()
    {
        // Arrange
        var chatMessage = new ChatMessageContent(AuthorRole.User, "");

        // Act
        var openRouterMessage = CallCreateOpenRouterMessage(chatMessage);

        // Assert
        Assert.Equal("user", openRouterMessage.Role);
        Assert.Null(openRouterMessage.Content);
    }

    [Fact]
    public void CreateOpenRouterMessage_WithFunctionCallContent_HandlesFunctionCalls()
    {
        // Arrange
        var chatMessage = new ChatMessageContent(AuthorRole.Assistant, "I'll help you with that.");
        var functionCall = new FunctionCallContent("TestFunction", "TestPlugin", "call123", new KernelArguments { ["param1"] = "value1" });
        chatMessage.Items.Add(functionCall);

        // Act
        var openRouterMessage = CallCreateOpenRouterMessage(chatMessage);

        // Assert
        Assert.Equal("assistant", openRouterMessage.Role);
        Assert.Equal("I'll help you with that.", openRouterMessage.Content);
        Assert.NotNull(openRouterMessage.ToolCalls);
        Assert.Single(openRouterMessage.ToolCalls);
        Assert.Equal("TestPlugin-TestFunction", openRouterMessage.ToolCalls[0].Function.Name);
    }

    [Fact]
    public void CreateOpenRouterMessage_WithFunctionResultContent_HandlesFunctionResult()
    {
        // Arrange
        var chatMessage = new ChatMessageContent(AuthorRole.Tool, "");
        var functionResult = new FunctionResultContent("TestFunction", "TestPlugin", "call123", "Function result");
        chatMessage.Items.Add(functionResult);

        // Act
        var openRouterMessage = CallCreateOpenRouterMessage(chatMessage);

        // Assert
        Assert.Equal("tool", openRouterMessage.Role);
        Assert.Equal("Function result", openRouterMessage.Content);
        Assert.Equal("call123", openRouterMessage.ToolCallId);
    }

    [Fact]
    public void ExtractTextFromContent_WithStringContent_ReturnsString()
    {
        // Arrange
        var content = "Hello, world!";

        // Act
        var result = CallExtractTextFromContent(content);

        // Assert
        Assert.Equal("Hello, world!", result);
    }

    [Fact]
    public void ExtractTextFromContent_WithContentArray_ReturnsJoinedText()
    {
        // Arrange
        var contentItems = new OpenRouterContentItem[]
        {
            OpenRouterContentHelper.CreateText("Hello, "),
            OpenRouterContentHelper.CreateText("world!")
        };

        // Act
        var result = CallExtractTextFromContent(contentItems);

        // Assert
        Assert.Equal("Hello, world!", result);
    }

    [Fact]
    public void ExtractTextFromContent_WithMixedContentArray_ReturnsOnlyText()
    {
        // Arrange
        var contentItems = new OpenRouterContentItem[]
        {
            OpenRouterContentHelper.CreateText("Text before image"),
            OpenRouterContentHelper.CreateImageUrl("https://example.com/image.jpg"),
            OpenRouterContentHelper.CreateText("Text after image")
        };

        // Act
        var result = CallExtractTextFromContent(contentItems);

        // Assert
        Assert.Equal("Text before imageText after image", result);
    }

    [Fact]
    public void ExtractTextFromContent_WithNullContent_ReturnsNull()
    {
        // Act
        var result = CallExtractTextFromContent(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ConvertRole_AllRoles_ConvertsCorrectly()
    {
        // Act & Assert
        Assert.Equal("user", CallConvertRole(AuthorRole.User));
        Assert.Equal("assistant", CallConvertRole(AuthorRole.Assistant));
        Assert.Equal("system", CallConvertRole(AuthorRole.System));
        Assert.Equal("tool", CallConvertRole(AuthorRole.Tool));
    }

    [Fact]
    public void ParseAuthorRole_AllRoles_ParsesCorrectly()
    {
        // Act & Assert
        Assert.Equal(AuthorRole.User, CallParseAuthorRole("user"));
        Assert.Equal(AuthorRole.Assistant, CallParseAuthorRole("assistant"));
        Assert.Equal(AuthorRole.System, CallParseAuthorRole("system"));
        Assert.Equal(AuthorRole.Tool, CallParseAuthorRole("tool"));
        Assert.Equal(AuthorRole.Assistant, CallParseAuthorRole("unknown"));
        Assert.Equal(AuthorRole.Assistant, CallParseAuthorRole(null));
    }

    // Helper methods to call private static methods using reflection
    private static OpenRouterMessage CallCreateOpenRouterMessage(ChatMessageContent message)
    {
        var method = typeof(OpenRouterChatCompletionService).GetMethod("CreateOpenRouterMessage", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (OpenRouterMessage)method.Invoke(null, new object[] { message })!;
    }

    private static string? CallExtractTextFromContent(object? content)
    {
        var method = typeof(OpenRouterChatCompletionService).GetMethod("ExtractTextFromContent", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string?)method.Invoke(null, new object?[] { content });
    }

    private static string CallConvertRole(AuthorRole role)
    {
        var method = typeof(OpenRouterChatCompletionService).GetMethod("ConvertRole", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (string)method.Invoke(null, new object[] { role })!;
    }

    private static AuthorRole CallParseAuthorRole(string? role)
    {
        var method = typeof(OpenRouterChatCompletionService).GetMethod("ParseAuthorRole", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return (AuthorRole)method.Invoke(null, new object?[] { role })!;
    }
}