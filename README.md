# SemanticKernel.Connectors.OpenRouter

OpenRouter connector for Microsoft Semantic Kernel. Provides chat completion and text generation services using OpenRouter's unified API to access hundreds of AI models through a single interface.

## Features

- **IChatCompletionService** - Full chat completion support with message history
- **ITextGenerationService** - Simple text generation from prompts  
- **Streaming Support** - Real-time streaming responses for both services
- **OpenRouter API** - Access 100+ models through OpenRouter's unified interface
- **Model Selection** - Support for model routing and provider preferences
- **Error Handling** - Comprehensive error handling with custom exceptions
- **Observability** - Full OpenTelemetry integration with metrics, tracing, and logging
- **Real-time Metrics** - Automatic cost, latency, and token usage tracking from OpenRouter

## Installation

```bash
# From NuGet.org (when published)
dotnet add package SemanticKernel.Connectors.OpenRouter

# From GitHub Packages
dotnet add package flexwie.SemanticKernel.Connectors.OpenRouter --source https://nuget.pkg.github.com/flexwie/index.json
```

## Quick Start

### Basic Setup

```csharp
using SemanticKernel.Connectors.OpenRouter.Extensions;
using Microsoft.SemanticKernel;

// Using builder extensions (recommended)
var kernel = Kernel.CreateBuilder()
    .AddOpenRouterChatCompletion(
        apiKey: "your-openrouter-api-key",
        modelId: "openai/gpt-3.5-turbo")
    .Build();

// Or create service directly
var chatService = new OpenRouterChatCompletionService(
    apiKey: "your-openrouter-api-key",
    modelId: "openai/gpt-3.5-turbo"
);
```

### Chat Completion

```csharp
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("You are a helpful assistant.");
chatHistory.AddUserMessage("What is the capital of France?");

var response = await chatService.GetChatMessageContentsAsync(chatHistory);
Console.WriteLine(response[0].Content);
```

### Streaming Chat

```csharp
await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory))
{
    Console.Write(chunk.Content);
}
```

### Text Generation

```csharp
var response = await chatService.GetTextContentsAsync("Explain quantum computing");
Console.WriteLine(response[0].Text);
```

### Advanced Configuration

```csharp
var settings = new OpenRouterExecutionSettings
{
    ModelId = "anthropic/claude-3-haiku",
    Temperature = 0.7,
    MaxTokens = 1000,
    TopP = 0.9,
    Models = new[] { "anthropic/claude-3-haiku", "openai/gpt-4" }, // Fallback models
    Provider = new { order = new[] { "Anthropic", "OpenAI" } }
};

var response = await chatService.GetChatMessageContentsAsync(
    chatHistory, 
    executionSettings: settings
);
```

## Observability & Metrics

The connector provides comprehensive observability through OpenTelemetry with real-time metrics fetched from OpenRouter's generation endpoint.

### Telemetry Configuration

```csharp
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry with OpenRouter metrics
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics.AddMeter("Microsoft.SemanticKernel.Connectors.OpenRouter");
    })
    .WithTracing(tracing =>
    {
        tracing.AddSource("Microsoft.SemanticKernel.Connectors.OpenRouter");
    });
```

### Available Metrics

#### Duration Metrics
- `semantic_kernel.connectors.openrouter.chat_completion.duration` - Chat completion request duration
- `semantic_kernel.connectors.openrouter.streaming_chat_completion.duration` - Streaming completion duration  
- `semantic_kernel.connectors.openrouter.text_generation.duration` - Text generation duration
- `semantic_kernel.connectors.openrouter.generation_time` - Actual model generation time (from OpenRouter)
- `semantic_kernel.connectors.openrouter.latency` - Network latency (from OpenRouter)

#### Token Usage Metrics
- `semantic_kernel.connectors.openrouter.tokens.prompt` - Normalized prompt tokens
- `semantic_kernel.connectors.openrouter.tokens.completion` - Normalized completion tokens
- `semantic_kernel.connectors.openrouter.tokens.total` - Total normalized tokens
- `semantic_kernel.connectors.openrouter.tokens.native_prompt` - Native prompt tokens (provider-specific)
- `semantic_kernel.connectors.openrouter.tokens.native_completion` - Native completion tokens
- `semantic_kernel.connectors.openrouter.tokens.native_total` - Total native tokens

#### Cost Metrics
- `semantic_kernel.connectors.openrouter.cost.actual` - Real cost in USD (from OpenRouter)
- `semantic_kernel.connectors.openrouter.cost.upstream` - Upstream provider cost for BYOK

#### Request Metrics
- `semantic_kernel.connectors.openrouter.requests.count` - Total request count
- `semantic_kernel.connectors.openrouter.requests.errors` - Failed request count

### Activity Tracing

Each request creates OpenTelemetry activities with detailed attributes:

```csharp
// Activity names
- openrouter.chat_completion
- openrouter.streaming_chat_completion  
- openrouter.text_generation

// Key attributes
- semantic_kernel.model_id
- semantic_kernel.ai_provider: "openrouter"
- semantic_kernel.operation_type
- semantic_kernel.is_streaming
- semantic_kernel.request_id
```

### Real-time Metrics Collection

The connector automatically fetches detailed generation metrics from OpenRouter after each request completion (non-blocking):

- **Actual Costs** - Real USD costs based on provider pricing
- **Native Token Counts** - Provider-specific tokenization (more accurate than normalized)
- **Generation Time** - Time spent by the model generating the response
- **Network Latency** - Round-trip network time
- **Provider Information** - Which upstream provider handled the request

## Configuration Options

### OpenRouterExecutionSettings

| Property | Type | Description |
|----------|------|-------------|
| `ModelId` | string | Primary model to use |
| `MaxTokens` | int? | Maximum tokens to generate |
| `Temperature` | double? | Sampling temperature (0-2) |
| `TopP` | double? | Top-p sampling |
| `TopK` | int? | Top-k sampling |
| `FrequencyPenalty` | double? | Frequency penalty |
| `PresencePenalty` | double? | Presence penalty |
| `RepetitionPenalty` | double? | Repetition penalty |
| `StopSequences` | string[]? | Stop sequences |
| `Models` | string[]? | Fallback models |
| `Provider` | object? | Provider preferences |

### Builder Extensions

```csharp
// Chat completion only
var kernel = Kernel.CreateBuilder()
    .AddOpenRouterChatCompletion("api-key", "model-id")
    .Build();

// Text generation only
var kernel = Kernel.CreateBuilder()
    .AddOpenRouterTextGeneration("api-key", "model-id")
    .Build();

// Both services (recommended)
var kernel = Kernel.CreateBuilder()
    .AddOpenRouter("api-key", "model-id")
    .Build();

// With all options
var kernel = Kernel.CreateBuilder()
    .AddOpenRouter(
        apiKey: "api-key",
        modelId: "model-id",
        baseUrl: new Uri("https://custom-proxy.com/api/v1"),
        serviceId: "my-openrouter-service",
        httpClient: customHttpClient)
    .Build();

// Multiple services
var kernel = Kernel.CreateBuilder()
    .AddOpenRouter("api-key-1", "openai/gpt-4", serviceId: "gpt4-service")
    .AddOpenRouter("api-key-2", "anthropic/claude-3-opus", serviceId: "claude-service")
    .Build();
```

### Dependency Injection

```csharp
// In Program.cs or Startup.cs
builder.Services.AddOpenRouter("api-key", "model-id");

// Or add to existing service collection
services.AddOpenRouterChatCompletion("api-key", "model-id");
services.AddOpenRouterTextGeneration("api-key", "model-id");
```

### Direct Service Construction

```csharp
// Minimal setup
var service = new OpenRouterChatCompletionService("api-key", "model-id");

// With custom HTTP client
var httpClient = new HttpClient();
var service = new OpenRouterChatCompletionService(
    apiKey: "api-key",
    modelId: "model-id", 
    httpClient: httpClient
);

// With custom base URL and logging
var service = new OpenRouterChatCompletionService(
    apiKey: "api-key",
    modelId: "model-id",
    baseUrl: new Uri("https://custom-proxy.com/api/v1"),
    logger: loggerFactory.CreateLogger<OpenRouterChatCompletionService>()
);
```

## Popular Models

OpenRouter supports hundreds of models. Here are some popular choices:

```csharp
// OpenAI Models
"openai/gpt-4-turbo"
"openai/gpt-3.5-turbo"

// Anthropic Models  
"anthropic/claude-3-opus"
"anthropic/claude-3-sonnet"
"anthropic/claude-3-haiku"

// Google Models
"google/gemini-pro"
"google/palm-2-chat-bison"

// Meta Models
"meta-llama/llama-2-70b-chat"

// Cohere Models
"cohere/command"
"cohere/command-nightly"
```

## Error Handling

The connector includes comprehensive error handling:

```csharp
try 
{
    var response = await chatService.GetChatMessageContentsAsync(chatHistory);
}
catch (OpenRouterException ex)
{
    Console.WriteLine($"OpenRouter API Error: {ex.Message}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Response: {ex.ResponseContent}");
}
```

## Getting an API Key

1. Sign up at [OpenRouter](https://openrouter.ai)
2. Navigate to the API Keys section
3. Create a new API key
4. Add credits to your account for usage

## Requirements

- .NET 8.0 or later
- Microsoft.SemanticKernel 1.60.0 or later
- Valid OpenRouter API key

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests to the [GitHub repository](https://github.com/flexwie/semantickernel-openrouter).

## License

MIT License - see LICENSE file for details.