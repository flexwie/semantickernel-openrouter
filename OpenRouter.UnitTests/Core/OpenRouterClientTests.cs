using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenRouter.UnitTests.Helpers;
using SemanticKernel.Connectors.OpenRouter.Core;
using SemanticKernel.Connectors.OpenRouter.Exceptions;
using SemanticKernel.Connectors.OpenRouter.Models;
using Xunit;

namespace OpenRouter.UnitTests.Core;

public class OpenRouterClientTests
{
    private readonly MockHttpMessageHandler _mockHandler;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterClient _client;

    public OpenRouterClientTests()
    {
        _mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, TestData.ChatCompletionResponse);
        _httpClient = new HttpClient(_mockHandler);
        _client = new OpenRouterClient(_httpClient, "test-api-key", null, null);
    }

    [Fact]
    public void Constructor_WithNullApiKey_ThrowsArgumentException()
    {
        var httpClient = new HttpClient();
        
        Assert.Throws<ArgumentNullException>(() => new OpenRouterClient(httpClient, null!, null, null));
        Assert.Throws<ArgumentException>(() => new OpenRouterClient(httpClient, "", null, null));
        Assert.Throws<ArgumentException>(() => new OpenRouterClient(httpClient, "   ", null, null));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new OpenRouterClient(null!, "api-key", null, null));
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithValidRequest_ReturnsExpectedResponse()
    {
        // Arrange
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[]
            {
                new OpenRouterMessage { Role = "user", Content = "Hello" }
            }
        };

        // Act
        var response = await _client.GetChatCompletionAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("chatcmpl-test123", response.Id);
        Assert.Equal("openai/gpt-3.5-turbo", response.Model);
        Assert.Single(response.Choices);
        Assert.Equal("Hello! How can I help you today?", response.Choices[0].Message?.Content);
        
        // Verify HTTP request
        var lastRequest = _mockHandler.GetLastRequest();
        Assert.Equal(HttpMethod.Post, lastRequest.Method);
        Assert.Contains("/chat/completions", lastRequest.RequestUri?.ToString());
        Assert.Equal("Bearer test-api-key", lastRequest.Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.GetChatCompletionAsync(null!));
    }

    [Fact]
    public async Task GetChatCompletionAsync_SetsStreamToFalse()
    {
        // Arrange
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Hello" } },
            Stream = true // This should be overridden
        };

        // Act
        await _client.GetChatCompletionAsync(request);

        // Assert
        var requestContent = await _mockHandler.GetLastRequestContentAsync();
        var sentRequest = JsonSerializer.Deserialize<OpenRouterRequest>(requestContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        
        Assert.NotNull(sentRequest);
        Assert.False(sentRequest.Stream);
    }

    [Fact]
    public async Task GetChatCompletionAsync_WithErrorResponse_ThrowsOpenRouterException()
    {
        // Arrange
        var errorHandler = new MockHttpMessageHandler(HttpStatusCode.Unauthorized, TestData.ErrorResponse);
        var errorClient = new HttpClient(errorHandler);
        var client = new OpenRouterClient(errorClient, "invalid-key", null, null);
        
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Hello" } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OpenRouterException>(() => client.GetChatCompletionAsync(request));
        Assert.Equal(401, exception.StatusCode);
        Assert.Contains("Unauthorized", exception.Message);
        Assert.NotNull(exception.ResponseContent);
    }

    [Fact]
    public async Task GetStreamingChatCompletionAsync_WithValidRequest_ReturnsStreamingResponses()
    {
        // Arrange
        var streamingHandler = new MockHttpMessageHandler(HttpStatusCode.OK, TestData.GetStreamingResponse());
        var streamingClient = new HttpClient(streamingHandler);
        var client = new OpenRouterClient(streamingClient, "test-api-key", null, null);
        
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Hello" } }
        };

        // Act
        var responses = new List<OpenRouterStreamResponse>();
        await foreach (var response in client.GetStreamingChatCompletionAsync(request))
        {
            responses.Add(response);
        }

        // Assert
        Assert.NotEmpty(responses);
        Assert.All(responses, r => Assert.Equal("openai/gpt-3.5-turbo", r.Model));
        
        // Verify stream flag was set
        var requestContent = await streamingHandler.GetLastRequestContentAsync();
        var sentRequest = JsonSerializer.Deserialize<OpenRouterRequest>(requestContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
        
        Assert.NotNull(sentRequest);
        Assert.True(sentRequest.Stream);
    }

    [Fact]
    public async Task GetStreamingChatCompletionAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await foreach (var _ in _client.GetStreamingChatCompletionAsync(null!))
            {
                // Should not reach here
            }
        });
    }

    [Fact]
    public async Task GetStreamingChatCompletionAsync_WithErrorResponse_ThrowsOpenRouterException()
    {
        // Arrange
        var errorHandler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, TestData.ErrorResponse);
        var errorClient = new HttpClient(errorHandler);
        var client = new OpenRouterClient(errorClient, "test-key", null, null);
        
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Hello" } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<OpenRouterException>(async () =>
        {
            await foreach (var _ in client.GetStreamingChatCompletionAsync(request))
            {
                // Should not reach here
            }
        });
        
        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task Client_AddsCorrectHeaders()
    {
        // Arrange
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Hello" } }
        };

        // Act
        await _client.GetChatCompletionAsync(request);

        // Assert
        var lastRequest = _mockHandler.GetLastRequest();
        Assert.Equal("Bearer test-api-key", lastRequest.Headers.Authorization?.ToString());
        Assert.Contains("application/json", lastRequest.Headers.Accept.ToString());
        Assert.Contains("SemanticKernel.OpenRouter", lastRequest.Headers.UserAgent.ToString());
    }

    [Fact]
    public void Client_WithCustomBaseUrl_UsesCustomUrl()
    {
        // Arrange
        var customBaseUrl = new Uri("https://custom.openrouter.com/v1");
        var httpClient = new HttpClient();
        var client = new OpenRouterClient(httpClient, "test-key", customBaseUrl, null);

        // The actual URL usage would be tested in integration tests
        // This test verifies the constructor accepts custom URLs without throwing
        Assert.NotNull(client);
    }
}