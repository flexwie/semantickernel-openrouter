using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenRouter.UnitTests.Helpers;
using SemanticKernel.Connectors.OpenRouter.Models;
using SemanticKernel.Connectors.OpenRouter.Services;
using Xunit;

namespace OpenRouter.UnitTests.Services;

public class OpenRouterChatCompletionServiceTests
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterChatCompletionService _service;

    public OpenRouterChatCompletionServiceTests()
    {
        _mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, TestData.ChatCompletionResponse);
        _httpClient = new HttpClient(_mockHandler);
        _service = new OpenRouterChatCompletionService(
            "test-api-key",
            "openai/gpt-3.5-turbo",
            httpClient: _httpClient);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesService()
    {
        var service = new OpenRouterChatCompletionService("api-key", "model-id");
        Assert.NotNull(service);
        Assert.NotNull(service.Attributes);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => new OpenRouterChatCompletionService(null!, "model"));
        Assert.Throws<ArgumentException>(() => new OpenRouterChatCompletionService("", "model"));
        Assert.Throws<ArgumentException>(() => new OpenRouterChatCompletionService("   ", "model"));
    }

    [Fact]
    public void Constructor_WithModelId_SetsModelIdInAttributes()
    {
        var modelId = "openai/gpt-4";
        var service = new OpenRouterChatCompletionService("api-key", modelId);
        
        Assert.True(service.Attributes.ContainsKey("ModelId"));
        Assert.Equal(modelId, service.Attributes["ModelId"]);
    }

    [Fact]
    public void Constructor_WithoutModelId_DoesNotSetModelIdInAttributes()
    {
        var service = new OpenRouterChatCompletionService("api-key");
        
        Assert.False(service.Attributes.ContainsKey("ModelId"));
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_WithValidChatHistory_ReturnsChatMessageContent()
    {
        // Arrange
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        var result = await _service.GetChatMessageContentsAsync(chatHistory);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var message = result[0];
        Assert.Equal(AuthorRole.Assistant, message.Role);
        Assert.Equal("Hello! How can I help you today?", message.Content);
        Assert.Equal("openai/gpt-3.5-turbo", message.ModelId);
        Assert.NotNull(message.Metadata);
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_WithNullChatHistory_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.GetChatMessageContentsAsync(null!));
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_WithExecutionSettings_UsesSettings()
    {
        // Arrange
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");
        
        var settings = new OpenRouterExecutionSettings
        {
            ModelId = "custom-model",
            Temperature = 0.8,
            MaxTokens = 100
        };

        // Act
        await _service.GetChatMessageContentsAsync(chatHistory, settings);

        // Assert
        var requestContent = await _mockHandler.GetLastRequestContentAsync();
        Assert.Contains("custom-model", requestContent);
        Assert.Contains("0.8", requestContent);
        Assert.Contains("100", requestContent);
    }

    [Fact]
    public async Task GetChatMessageContentsAsync_WithoutModelId_ThrowsArgumentException()
    {
        // Arrange
        var serviceWithoutModel = new OpenRouterChatCompletionService("api-key", httpClient: _httpClient);
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            serviceWithoutModel.GetChatMessageContentsAsync(chatHistory));
    }

    [Fact]
    public async Task GetStreamingChatMessageContentsAsync_WithValidChatHistory_ReturnsStreamingContent()
    {
        // Arrange
        var streamingHandler = new MockHttpMessageHandler(HttpStatusCode.OK, TestData.GetStreamingResponse());
        var streamingClient = new HttpClient(streamingHandler);
        var streamingService = new OpenRouterChatCompletionService(
            "test-api-key", 
            "openai/gpt-3.5-turbo", 
            httpClient: streamingClient);
        
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        var chunks = new List<StreamingChatMessageContent>();
        await foreach (var chunk in streamingService.GetStreamingChatMessageContentsAsync(chatHistory))
        {
            chunks.Add(chunk);
        }

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.Equal("openai/gpt-3.5-turbo", chunk.ModelId));
    }

    [Fact]
    public async Task GetStreamingChatMessageContentsAsync_WithNullChatHistory_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in _service.GetStreamingChatMessageContentsAsync(null!))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task GetTextContentsAsync_WithValidPrompt_ReturnsTextContent()
    {
        // Arrange
        var prompt = "What is the meaning of life?";

        // Act
        var result = await _service.GetTextContentsAsync(prompt);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        
        var textContent = result[0];
        Assert.Equal("Hello! How can I help you today?", textContent.Text);
        Assert.Equal("openai/gpt-3.5-turbo", textContent.ModelId);
    }

    [Fact]
    public async Task GetTextContentsAsync_WithNullPrompt_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetTextContentsAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTextContentsAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetTextContentsAsync("   "));
    }

    [Fact]
    public async Task GetStreamingTextContentsAsync_WithValidPrompt_ReturnsStreamingTextContent()
    {
        // Arrange
        var streamingHandler = new MockHttpMessageHandler(HttpStatusCode.OK, TestData.GetStreamingResponse());
        var streamingClient = new HttpClient(streamingHandler);
        var streamingService = new OpenRouterChatCompletionService(
            "test-api-key", 
            "openai/gpt-3.5-turbo", 
            httpClient: streamingClient);
        
        var prompt = "Tell me a joke";

        // Act
        var chunks = new List<StreamingTextContent>();
        await foreach (var chunk in streamingService.GetStreamingTextContentsAsync(prompt))
        {
            chunks.Add(chunk);
        }

        // Assert
        Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => Assert.Equal("openai/gpt-3.5-turbo", chunk.ModelId));
    }

    [Fact]
    public async Task GetStreamingTextContentsAsync_WithNullPrompt_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in _service.GetStreamingTextContentsAsync(null!))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task Service_ConvertsRolesCorrectly()
    {
        // Arrange
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage("System message");
        chatHistory.AddUserMessage("User message");
        chatHistory.AddAssistantMessage("Assistant message");

        // Act
        await _service.GetChatMessageContentsAsync(chatHistory);

        // Assert
        var requestContent = await _mockHandler.GetLastRequestContentAsync();
        Assert.Contains("\"role\":\"system\"", requestContent);
        Assert.Contains("\"role\":\"user\"", requestContent);
        Assert.Contains("\"role\":\"assistant\"", requestContent);
    }

    [Fact]
    public async Task Service_IncludesUsageMetadata()
    {
        // Arrange
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage("Hello");

        // Act
        var result = await _service.GetChatMessageContentsAsync(chatHistory);

        // Assert
        var message = result[0];
        Assert.NotNull(message.Metadata);
        Assert.True(message.Metadata.ContainsKey("Usage"));
        Assert.True(message.Metadata.ContainsKey("PromptTokens"));
        Assert.True(message.Metadata.ContainsKey("CompletionTokens"));
        Assert.True(message.Metadata.ContainsKey("TotalTokens"));
    }

    [Fact]
    public void Service_WithCustomLogger_AcceptsLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<OpenRouterChatCompletionService>();
        
        var service = new OpenRouterChatCompletionService(
            "api-key", 
            "model", 
            logger: logger);
        
        Assert.NotNull(service);
    }

    [Fact]
    public void Service_WithCustomBaseUrl_AcceptsBaseUrl()
    {
        var customUrl = new Uri("https://custom.openrouter.com/v1");
        
        var service = new OpenRouterChatCompletionService(
            "api-key", 
            "model", 
            baseUrl: customUrl);
        
        Assert.NotNull(service);
    }
}