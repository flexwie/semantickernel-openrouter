using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

public sealed class OpenRouterRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required OpenRouterMessage[] Messages { get; set; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    [JsonPropertyName("frequency_penalty")]
    public double? FrequencyPenalty { get; set; }

    [JsonPropertyName("presence_penalty")]
    public double? PresencePenalty { get; set; }

    [JsonPropertyName("repetition_penalty")]
    public double? RepetitionPenalty { get; set; }

    [JsonPropertyName("stop")]
    public string[]? StopSequences { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("models")]
    public string[]? Models { get; set; }

    [JsonPropertyName("provider")]
    public object? Provider { get; set; }

    [JsonPropertyName("tools")]
    public OpenRouterTool[]? Tools { get; set; }

    [JsonPropertyName("tool_choice")]
    public object? ToolChoice { get; set; }

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("response_format")]
    public object? ResponseFormat { get; set; }

    [JsonPropertyName("logit_bias")]
    public Dictionary<int, int>? LogitBias { get; set; }

    [JsonPropertyName("user")]
    public string? User { get; set; }

    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }
}

public sealed class OpenRouterMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tool_calls")]
    public OpenRouterToolCall[]? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}