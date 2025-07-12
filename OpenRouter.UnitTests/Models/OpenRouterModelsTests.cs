using System.Text.Json;
using SemanticKernel.Connectors.OpenRouter.Models;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    [Fact]
    public void OpenRouterRequest_SerializesCorrectly()
    {
        var request = new OpenRouterRequest
        {
            Model = "openai/gpt-3.5-turbo",
            Messages = new[]
            {
                new OpenRouterMessage { Role = "user", Content = "Hello" }
            },
            MaxTokens = 100,
            Temperature = 0.7,
            Stream = true
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        
        Assert.Contains("\"model\":\"openai/gpt-3.5-turbo\"", json);
        Assert.Contains("\"max_tokens\":100", json);
        Assert.Contains("\"temperature\":0.7", json);
        Assert.Contains("\"stream\":true", json);
        Assert.Contains("\"messages\"", json);
        Assert.Contains("\"role\":\"user\"", json);
        Assert.Contains("\"content\":\"Hello\"", json);
    }

    [Fact]
    public void OpenRouterRequest_DeserializesCorrectly()
    {
        var json = """
        {
            "model": "openai/gpt-3.5-turbo",
            "messages": [
                {
                    "role": "user",
                    "content": "Hello"
                }
            ],
            "max_tokens": 100,
            "temperature": 0.7,
            "stream": true
        }
        """;

        var request = JsonSerializer.Deserialize<OpenRouterRequest>(json, JsonOptions);
        
        Assert.NotNull(request);
        Assert.Equal("openai/gpt-3.5-turbo", request.Model);
        Assert.Single(request.Messages);
        Assert.Equal("user", request.Messages[0].Role);
        Assert.Equal("Hello", request.Messages[0].Content);
        Assert.Equal(100, request.MaxTokens);
        Assert.Equal(0.7, request.Temperature);
        Assert.True(request.Stream);
    }

    [Fact]
    public void OpenRouterMessage_WithName_SerializesCorrectly()
    {
        var message = new OpenRouterMessage
        {
            Role = "assistant",
            Content = "Hello there!",
            Name = "Assistant"
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        
        Assert.Contains("\"role\":\"assistant\"", json);
        Assert.Contains("\"content\":\"Hello there!\"", json);
        Assert.Contains("\"name\":\"Assistant\"", json);
    }

    [Fact]
    public void OpenRouterResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion",
            "created": 1677652288,
            "model": "openai/gpt-3.5-turbo",
            "choices": [
                {
                    "index": 0,
                    "message": {
                        "role": "assistant",
                        "content": "Hello!"
                    },
                    "finish_reason": "stop"
                }
            ],
            "usage": {
                "prompt_tokens": 10,
                "completion_tokens": 5,
                "total_tokens": 15
            }
        }
        """;

        var response = JsonSerializer.Deserialize<OpenRouterResponse>(json, JsonOptions);
        
        Assert.NotNull(response);
        Assert.Equal("chatcmpl-123", response.Id);
        Assert.Equal("chat.completion", response.Object);
        Assert.Equal(1677652288, response.Created);
        Assert.Equal("openai/gpt-3.5-turbo", response.Model);
        Assert.Single(response.Choices);
        
        var choice = response.Choices[0];
        Assert.Equal(0, choice.Index);
        Assert.Equal("stop", choice.FinishReason);
        Assert.NotNull(choice.Message);
        Assert.Equal("assistant", choice.Message.Role);
        Assert.Equal("Hello!", choice.Message.Content);
        
        Assert.NotNull(response.Usage);
        Assert.Equal(10, response.Usage.PromptTokens);
        Assert.Equal(5, response.Usage.CompletionTokens);
        Assert.Equal(15, response.Usage.TotalTokens);
    }

    [Fact]
    public void OpenRouterStreamResponse_DeserializesCorrectly()
    {
        var json = """
        {
            "id": "chatcmpl-123",
            "object": "chat.completion.chunk",
            "created": 1677652288,
            "model": "openai/gpt-3.5-turbo",
            "choices": [
                {
                    "index": 0,
                    "delta": {
                        "role": "assistant",
                        "content": "Hello"
                    },
                    "finish_reason": null
                }
            ]
        }
        """;

        var response = JsonSerializer.Deserialize<OpenRouterStreamResponse>(json, JsonOptions);
        
        Assert.NotNull(response);
        Assert.Equal("chatcmpl-123", response.Id);
        Assert.Equal("chat.completion.chunk", response.Object);
        Assert.Equal(1677652288, response.Created);
        Assert.Equal("openai/gpt-3.5-turbo", response.Model);
        Assert.Single(response.Choices);
        
        var choice = response.Choices[0];
        Assert.Equal(0, choice.Index);
        Assert.Null(choice.FinishReason);
        Assert.NotNull(choice.Delta);
        Assert.Equal("assistant", choice.Delta.Role);
        Assert.Equal("Hello", choice.Delta.Content);
    }

    [Fact]
    public void OpenRouterRequest_WithOptionalFields_HandlesNulls()
    {
        var request = new OpenRouterRequest
        {
            Model = "test-model",
            Messages = new[]
            {
                new OpenRouterMessage { Role = "user", Content = "Test" }
            }
            // All other fields are null/default
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<OpenRouterRequest>(json, JsonOptions);
        
        Assert.NotNull(deserialized);
        Assert.Equal("test-model", deserialized.Model);
        Assert.Single(deserialized.Messages);
        Assert.Null(deserialized.MaxTokens);
        Assert.Null(deserialized.Temperature);
        Assert.Null(deserialized.TopP);
        Assert.False(deserialized.Stream); // bool defaults to false
    }

    [Fact]
    public void OpenRouterRequest_WithAllOptionalFields_SerializesCorrectly()
    {
        var request = new OpenRouterRequest
        {
            Model = "test-model",
            Messages = new[] { new OpenRouterMessage { Role = "user", Content = "Test" } },
            MaxTokens = 1000,
            Temperature = 0.8,
            TopP = 0.9,
            TopK = 50,
            FrequencyPenalty = 0.1,
            PresencePenalty = 0.2,
            RepetitionPenalty = 1.1,
            StopSequences = new[] { "stop" },
            Stream = true,
            Models = new[] { "fallback-model" },
            Provider = new { test = "value" }
        };

        var json = JsonSerializer.Serialize(request, JsonOptions);
        
        Assert.Contains("\"max_tokens\":1000", json);
        Assert.Contains("\"temperature\":0.8", json);
        Assert.Contains("\"top_p\":0.9", json);
        Assert.Contains("\"top_k\":50", json);
        Assert.Contains("\"frequency_penalty\":0.1", json);
        Assert.Contains("\"presence_penalty\":0.2", json);
        Assert.Contains("\"repetition_penalty\":1.1", json);
        Assert.Contains("\"stop\":[\"stop\"]", json);
        Assert.Contains("\"stream\":true", json);
        Assert.Contains("\"models\":[\"fallback-model\"]", json);
        Assert.Contains("\"provider\":", json);
    }
}