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
# From NuGet.org
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
    Provider = new { order = new[] { "Anthropic", "OpenAI" } },

    // Enhanced configuration options
    Seed = 12345,                                    // Deterministic generation
    ResponseFormat = OpenRouterResponseFormat.JsonObject, // JSON mode
    User = "user-123",                               // User tracking
    MaxCompletionTokens = 800,                       // Separate completion limit
    Store = true,                                    // Store for analysis
    TopLogprobs = 5,                                 // Return token probabilities
    LogProbs = true,                                 // Include probability data
    ServiceTier = "auto",                            // Service tier optimization
    ParallelToolCalls = true,                        // Parallel function execution

    // Token bias examples
    LogitBias = OpenRouterLogitBias.Merge(
        OpenRouterLogitBias.Suppress(50256),         // Suppress specific tokens
        OpenRouterLogitBias.Favor(25, 100, 200)     // Favor other tokens
    ),

    // Request metadata
    Metadata = new Dictionary<string, object>
    {
        ["session_id"] = "session-123",
        ["user_type"] = "premium"
    }
};

var response = await chatService.GetChatMessageContentsAsync(
    chatHistory,
    executionSettings: settings
);
```

### JSON Mode and Structured Output

```csharp
// Simple JSON object mode
var jsonSettings = new OpenRouterExecutionSettings
{
    ModelId = "openai/gpt-4",
    ResponseFormat = OpenRouterResponseFormat.JsonObject
};

// Structured JSON with schema
var schema = new JsonSchema
{
    Type = "object",
    Properties = new Dictionary<string, JsonSchemaProperty>
    {
        ["name"] = new() { Type = "string", Description = "Person's name" },
        ["age"] = new() { Type = "integer", Description = "Person's age" },
        ["skills"] = new()
        {
            Type = "array",
            Items = new() { Type = "string" }
        }
    },
    Required = ["name", "age"]
};

var structuredSettings = new OpenRouterExecutionSettings
{
    ModelId = "openai/gpt-4",
    ResponseFormat = OpenRouterResponseFormat.JsonSchema("person", "A person object", schema)
};
```

### Deterministic Generation

```csharp
// Use the same seed for reproducible outputs
var deterministicSettings = new OpenRouterExecutionSettings
{
    ModelId = "openai/gpt-3.5-turbo",
    Seed = 42,
    Temperature = 0.7
};

// Multiple calls with same seed will produce identical results
var response1 = await chatService.GetChatMessageContentsAsync(chatHistory, deterministicSettings);
var response2 = await chatService.GetChatMessageContentsAsync(chatHistory, deterministicSettings);
// response1.Content == response2.Content (with high probability)
```

### Token Control and Bias

```csharp
var tokenControlSettings = new OpenRouterExecutionSettings
{
    ModelId = "openai/gpt-4",

    // Suppress unwanted tokens (e.g., prevent certain words)
    LogitBias = OpenRouterLogitBias.Merge(
        OpenRouterLogitBias.Suppress(50256, 628),    // Suppress specific token IDs
        OpenRouterLogitBias.Discourage(-50, 1000),   // Reduce likelihood of token 1000
        OpenRouterLogitBias.Encourage(2000, 3000)    // Increase likelihood of tokens
    ),

    // Token limits
    MaxTokens = 2000,              // Total token limit
    MaxCompletionTokens = 1500,    // Completion-specific limit

    // Probability information
    LogProbs = true,               // Include token probabilities
    TopLogprobs = 10              // Return top 10 alternatives per token
};
```

## Images and PDFs

OpenRouter supports multimodal conversations with images and PDF documents. Use vision-capable models like `gpt-4-vision-preview`, `claude-3-opus`, or `gemini-pro-vision`.

### Image Analysis

```csharp
using SemanticKernel.Connectors.OpenRouter.Core;

var chatHistory = new ChatHistory();

// Add a message with text and image
var message = new ChatMessageContent(AuthorRole.User, "What's in this image?");

// From URL
var imageContent = OpenRouterContentHelper.CreateImageUrl("https://example.com/image.jpg", "high");

// From file path
var imageFromFile = OpenRouterContentUtilities.CreateImageFromFile("/path/to/image.png", "low");

// From byte data
var imageBytes = File.ReadAllBytes("/path/to/image.jpg");
var imageFromBytes = OpenRouterContentUtilities.CreateImageFromBytes(imageBytes, "image/jpeg", "auto");

// Add image to message (using helper for URL example)
chatHistory.AddUserMessage("What's in this image?");
// For multimodal, create message manually:
var multimodalMessage = new ChatMessageContent(AuthorRole.User, "Analyze this image:");
// Note: Direct image addition to ChatMessageContent requires Semantic Kernel's ImageContent
// This example shows the utility methods available

var response = await chatService.GetChatMessageContentsAsync(chatHistory);
```

### PDF Document Analysis

```csharp
using SemanticKernel.Connectors.OpenRouter.Core;

var chatHistory = new ChatHistory();

// From file path
var pdfContent = OpenRouterContentUtilities.CreateFileFromPath(
    "/path/to/document.pdf",
    OpenRouterContentUtilities.PdfProcessingEngines.PdfText  // Free text extraction
);

// From byte data with OCR processing
var pdfBytes = File.ReadAllBytes("/path/to/scanned.pdf");
var pdfFromBytes = OpenRouterContentUtilities.CreateFileFromBytes(
    pdfBytes,
    "document.pdf",
    "application/pdf",
    OpenRouterContentUtilities.PdfProcessingEngines.MistralOcr  // $2 per 1,000 pages
);

// Add to chat (conceptual - actual implementation depends on Semantic Kernel content types)
chatHistory.AddUserMessage("Summarize this document:");

var response = await chatService.GetChatMessageContentsAsync(chatHistory);
```

### Supported Formats

**Images:**

- PNG (`image/png`)
- JPEG (`image/jpeg`, `image/jpg`)
- WebP (`image/webp`)

**Documents:**

- PDF (`application/pdf`)
- Plain text (`text/plain`)
- Microsoft Word (`application/msword`, `.docx`)

### Processing Engines

For PDF documents, choose the appropriate processing engine:

```csharp
// Free text extraction (best for clear text documents)
OpenRouterContentUtilities.PdfProcessingEngines.PdfText

// OCR processing (best for scanned documents, $2 per 1,000 pages)
OpenRouterContentUtilities.PdfProcessingEngines.MistralOcr

// Model-specific processing (varies by model)
OpenRouterContentUtilities.PdfProcessingEngines.Native
```

### Advanced Multimodal Usage

```csharp
// Create multimodal content programmatically
var contentItems = new List<OpenRouterContentItem>
{
    OpenRouterContentHelper.CreateText("Compare these documents:"),
    OpenRouterContentHelper.CreateFile("report1.pdf", pdfBase64Data1, "application/pdf", "pdf-text"),
    OpenRouterContentHelper.CreateImageUrl("https://example.com/chart.png", "high"),
    OpenRouterContentHelper.CreateFile("report2.pdf", pdfBase64Data2, "application/pdf", "pdf-text")
};

// Use with vision-capable models
var settings = new OpenRouterExecutionSettings
{
    ModelId = "anthropic/claude-3-opus",  // Vision-capable model
    MaxTokens = 4000,
    Temperature = 0.1  // Lower temperature for analytical tasks
};
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

| Property            | Type      | Description                |
| ------------------- | --------- | -------------------------- |
| `ModelId`           | string    | Primary model to use       |
| `MaxTokens`         | int?      | Maximum tokens to generate |
| `Temperature`       | double?   | Sampling temperature (0-2) |
| `TopP`              | double?   | Top-p sampling             |
| `TopK`              | int?      | Top-k sampling             |
| `FrequencyPenalty`  | double?   | Frequency penalty          |
| `PresencePenalty`   | double?   | Presence penalty           |
| `RepetitionPenalty` | double?   | Repetition penalty         |
| `StopSequences`     | string[]? | Stop sequences             |
| `Models`            | string[]? | Fallback models            |
| `Provider`          | object?   | Provider preferences       |

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
