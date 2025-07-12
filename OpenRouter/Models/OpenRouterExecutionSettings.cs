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
}