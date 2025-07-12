using System.Text.Json.Serialization;

namespace SemanticKernel.Connectors.OpenRouter.Models;

public sealed class OpenRouterGenerationResponse
{
    [JsonPropertyName("data")]
    public required OpenRouterGeneration Data { get; set; }
}

public sealed class OpenRouterGeneration
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("total_cost")]
    public double? TotalCost { get; set; }

    [JsonPropertyName("upstream_inference_cost")]
    public double? UpstreamInferenceCost { get; set; }

    [JsonPropertyName("cache_discount")]
    public double? CacheDiscount { get; set; }

    [JsonPropertyName("provider_name")]
    public string? ProviderName { get; set; }

    [JsonPropertyName("tokens_prompt")]
    public int TokensPrompt { get; set; }

    [JsonPropertyName("tokens_completion")]
    public int TokensCompletion { get; set; }

    [JsonPropertyName("native_tokens_prompt")]
    public int NativeTokensPrompt { get; set; }

    [JsonPropertyName("native_tokens_completion")]
    public int NativeTokensCompletion { get; set; }

    [JsonPropertyName("native_tokens_reasoning")]
    public int? NativeTokensReasoning { get; set; }

    [JsonPropertyName("num_media_prompt")]
    public int? NumMediaPrompt { get; set; }

    [JsonPropertyName("num_media_completion")]
    public int? NumMediaCompletion { get; set; }

    [JsonPropertyName("generation_time")]
    public double? GenerationTime { get; set; }

    [JsonPropertyName("latency")]
    public double? Latency { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    [JsonPropertyName("is_byok")]
    public bool IsByok { get; set; }

    [JsonPropertyName("streamed")]
    public bool Streamed { get; set; }

    [JsonPropertyName("cancelled")]
    public bool Cancelled { get; set; }

    public int GetTotalNativeTokens()
    {
        return NativeTokensPrompt + NativeTokensCompletion + (NativeTokensReasoning ?? 0);
    }

    public int GetTotalNormalizedTokens()
    {
        return TokensPrompt + TokensCompletion;
    }
}