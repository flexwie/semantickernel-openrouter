using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

public sealed class OpenRouterExecutionSettings : PromptExecutionSettings
{
    public static OpenRouterExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings)
    {
        return executionSettings switch
        {
            null => new OpenRouterExecutionSettings(),
            OpenRouterExecutionSettings settings => settings,
            _ => new OpenRouterExecutionSettings
            {
                ModelId = executionSettings.ModelId,
                ExtensionData = executionSettings.ExtensionData,
                ServiceId = executionSettings.ServiceId
            }
        };
    }

    [JsonPropertyName("model")]
    public string? Model
    {
        get => ModelId;
        set => ModelId = value;
    }

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

    /// <summary>
    /// Seed for deterministic generation. Use the same seed to get the same output.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Response format configuration. Can be "json_object" for JSON mode or other formats.
    /// </summary>
    [JsonPropertyName("response_format")]
    public object? ResponseFormat { get; set; }

    /// <summary>
    /// Bias values for specific tokens. Maps token IDs to bias values (-100 to 100).
    /// </summary>
    [JsonPropertyName("logit_bias")]
    public Dictionary<int, int>? LogitBias { get; set; }

    /// <summary>
    /// User identifier for tracking and monitoring purposes.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }

    /// <summary>
    /// Maximum number of tokens for the completion (separate from total max_tokens).
    /// </summary>
    [JsonPropertyName("max_completion_tokens")]
    public int? MaxCompletionTokens { get; set; }

    /// <summary>
    /// Whether to store the response for monitoring and analysis.
    /// </summary>
    [JsonPropertyName("store")]
    public bool? Store { get; set; }

    /// <summary>
    /// Metadata for request tracking and analysis.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Number of most likely tokens to return at each position (0-20).
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public int? TopLogprobs { get; set; }

    /// <summary>
    /// Whether to return log probabilities of output tokens.
    /// </summary>
    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    /// <summary>
    /// Service tier for latency and throughput optimization.
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    /// <summary>
    /// Whether tool calls can be executed in parallel.
    /// </summary>
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }
}