using Microsoft.Extensions.Logging;
using SemanticKernel.Connectors.OpenRouter.Exceptions;
using SemanticKernel.Connectors.OpenRouter.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace SemanticKernel.Connectors.OpenRouter.Core;

public sealed class OpenRouterClient
{
    private const string DefaultBaseUrl = "https://openrouter.ai/api/v1";
    private const string ChatCompletionsEndpoint = "/chat/completions";

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _apiKey;
    private readonly Uri _baseUrl;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public OpenRouterClient(HttpClient httpClient, string apiKey, Uri? baseUrl = null, ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey;
        _baseUrl = baseUrl ?? new Uri(DefaultBaseUrl);
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SemanticKernel.OpenRouter", "1.0.0"));
        }
    }

    public async Task<OpenRouterResponse> GetChatCompletionAsync(OpenRouterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Sending chat completion request to OpenRouter");

        request.Stream = false;
        var response = await SendRequestAsync<OpenRouterResponse>(ChatCompletionsEndpoint, request, cancellationToken);
        
        _logger.LogDebug("Received chat completion response from OpenRouter");

        // Fire and forget: fetch generation details for metrics (don't block main execution)
        if (!string.IsNullOrEmpty(response.Id))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await FetchAndRecordGenerationMetricsAsync(response.Id, request.Model, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch generation metrics for request {RequestId}", response.Id);
                }
            }, CancellationToken.None); // Use None to prevent cancellation affecting metrics collection
        }

        return response;
    }

    public async IAsyncEnumerable<OpenRouterStreamResponse> GetStreamingChatCompletionAsync(
        OpenRouterRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogDebug("Sending streaming chat completion request to OpenRouter");

        request.Stream = true;
        var httpRequest = CreateHttpRequest(ChatCompletionsEndpoint, request);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessStatusCodeAsync(httpResponse);

        using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? lastGenerationId = null;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data: ", StringComparison.Ordinal))
            {
                var data = line["data: ".Length..];
                
                if (data == "[DONE]")
                {
                    _logger.LogDebug("Streaming response completed");
                    
                    // Fire and forget: fetch generation details for metrics after streaming completes
                    if (!string.IsNullOrEmpty(lastGenerationId))
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await FetchAndRecordGenerationMetricsAsync(lastGenerationId, request.Model, true, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to fetch generation metrics for streaming request {RequestId}", lastGenerationId);
                            }
                        }, CancellationToken.None);
                    }
                    
                    yield break;
                }

                OpenRouterStreamResponse? streamResponse;
                try
                {
                    streamResponse = JsonSerializer.Deserialize<OpenRouterStreamResponse>(data, JsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize streaming response: {Data}", data);
                    continue;
                }

                if (streamResponse != null)
                {
                    // Track the generation ID for metrics collection
                    if (!string.IsNullOrEmpty(streamResponse.Id))
                    {
                        lastGenerationId = streamResponse.Id;
                    }
                    
                    yield return streamResponse;
                }
            }
        }
    }

    private async Task<T> SendRequestAsync<T>(string endpoint, object request, CancellationToken cancellationToken)
    {
        var httpRequest = CreateHttpRequest(endpoint, request);

        using var httpResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessStatusCodeAsync(httpResponse);

        var responseContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(responseContent, JsonOptions);
            return result ?? throw new OpenRouterException("Deserialized response was null");
        }
        catch (JsonException ex)
        {
            throw new OpenRouterException($"Failed to deserialize response: {ex.Message}", ex)
            {
                ResponseContent = responseContent
            };
        }
    }

    private HttpRequestMessage CreateHttpRequest(string endpoint, object request)
    {
        var requestUri = new Uri(_baseUrl, endpoint);
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };

        return httpRequest;
    }

    private static async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        
        throw new OpenRouterException($"OpenRouter API returned {response.StatusCode}: {response.ReasonPhrase}")
        {
            StatusCode = (int)response.StatusCode,
            ResponseContent = content
        };
    }

    private async Task FetchAndRecordGenerationMetricsAsync(string generationId, string modelId, bool isStreaming, CancellationToken cancellationToken)
    {
        try
        {
            // Small delay to ensure generation details are available
            await Task.Delay(100, cancellationToken);

            var generationUri = new Uri(_baseUrl, $"/generation?id={generationId}");
            using var request = new HttpRequestMessage(HttpMethod.Get, generationUri);
            
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Generation details not available for {GenerationId}, status: {StatusCode}", generationId, response.StatusCode);
                return;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var generationResponse = JsonSerializer.Deserialize<OpenRouterGenerationResponse>(content, JsonOptions);

            if (generationResponse?.Data != null)
            {
                var tags = OpenRouterTelemetry.CreateTags(
                    modelId, 
                    OpenRouterTelemetry.OperationTypes.ChatCompletion,
                    isStreaming,
                    generationId);

                OpenRouterTelemetry.RecordGenerationMetrics(tags, generationResponse.Data);
                
                _logger.LogDebug("Recorded generation metrics for {GenerationId}: Cost={Cost}, Tokens={Tokens}, Time={Time}s", 
                    generationId, 
                    generationResponse.Data.TotalCost,
                    generationResponse.Data.GetTotalNativeTokens(),
                    generationResponse.Data.GenerationTime);
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to fetch generation details for {GenerationId}", generationId);
        }
    }
}