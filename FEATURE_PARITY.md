# OpenRouter Connector Feature Parity with OpenAI

This document tracks the feature parity between our OpenRouter connector and Microsoft Semantic Kernel's OpenAI connector.

## **Current Implementation Status**

### ✅ **Implemented Services (33% coverage)**
- `IChatCompletionService` - Chat completion with message history
- `ITextGenerationService` - Simple text generation from prompts

### ❌ **Missing Services (67% coverage)**
- `ITextEmbeddingGenerationService` - **⚠️ NOT SUPPORTED BY OPENROUTER API**
- `ITextToImageService` - Image generation from text prompts  
- `IAudioToTextService` - Speech-to-text transcription
- `ITextToAudioService` - Text-to-speech synthesis
- File service interfaces - File upload/download/management

> **Note**: OpenRouter does not provide embeddings endpoints (`/v1/embeddings`). Only chat completions and text completions are supported.

---

## **Configuration Options**

### ✅ **Implemented in OpenRouterExecutionSettings (85% coverage)**
- `ModelId` - Primary model selection
- `MaxTokens` - Token generation limits
- `Temperature` - Sampling temperature (0-2)
- `TopP` - Top-p sampling
- `TopK` - Top-k sampling  
- `FrequencyPenalty` - Frequency penalty
- `PresencePenalty` - Presence penalty
- `RepetitionPenalty` - Repetition penalty
- `StopSequences` - Stop sequences
- `Models` - Fallback models array
- `Provider` - Provider preferences
- `Seed` - Deterministic generation
- `ResponseFormat` - JSON/text response formatting (with JsonObject and JsonSchema support)
- `LogitBias` - Token selection biases (with helper utilities)
- `User` - User identification for tracking
- `MaxCompletionTokens` - Separate completion token limits
- `Store` - Response storage preferences
- `Metadata` - Request metadata tracking
- `TopLogprobs` - Probability logging configuration
- `LogProbs` - Token probability information
- `ServiceTier` - Service level selection
- `ParallelToolCalls` - Parallel function execution

### ❌ **Missing Configuration Options (15% coverage)**
- `ReasoningEffort` - Reasoning intensity levels
- `Modalities` - Multi-modal response types
- `Audio` - Audio response configuration
- `WebSearch` - Web search capabilities

---

## **Content Types**

### ✅ **Implemented Content Types (70% coverage)**
- `ChatMessageContent` - Text-based chat messages
- `TextContent` - Simple text content
- `StreamingChatMessageContent` - Streaming chat responses
- `StreamingTextContent` - Streaming text responses
- `FunctionCallContent` - Function call representations
- `FunctionResultContent` - Function execution results
- **Multimodal Content Support** - Images and files via reflection-based content conversion
- **Image Processing** - PNG, JPEG, WebP support with URL and base64 data
- **File Processing** - PDF, Word documents with multiple processing engines
- **Content Arrays** - Mixed text, image, and file content in single messages

### ❌ **Missing Content Types (30% coverage)**
- `AudioContent` - Audio input/output handling (not supported by OpenRouter API)
- `EmbeddingContent` - Vector embeddings (not supported by OpenRouter API)
- Enhanced streaming content with finish reasons
- Native Semantic Kernel content type integration (using reflection workaround)

---

## **Function Calling**

### ✅ **Implemented Function Calling (75% coverage)**
- Function choice behaviors (Auto, None, Required)
- Automatic function invocation with max iterations
- Parameter schema generation from .NET types
- Function conversion utilities (SK ↔ OpenRouter)
- Tool call processing in streaming/non-streaming
- Function result handling in chat history
- Error handling with logging and telemetry

### ❌ **Missing Function Calling (25% coverage)**
- Advanced function validation
- Recursive call protection
- Enhanced tool choice behaviors
- Function metadata extensions

---

## **Builder Extensions & DI**

### ✅ **Implemented Extensions (40% coverage)**
- `AddOpenRouter()` - Both chat and text services
- `AddOpenRouterChatCompletion()` - Chat service only
- `AddOpenRouterTextGeneration()` - Text service only
- Service collection extensions
- Kernel builder extensions
- Multiple service registration support

### ❌ **Missing Extensions (60% coverage)**
- `AddOpenRouterTextEmbedding()` - Embedding service registration
- `AddOpenRouterTextToImage()` - Image generation service
- `AddOpenRouterTextToAudio()` - Audio generation service
- `AddOpenRouterAudioToText()` - Audio transcription service
- `AddOpenRouterFiles()` - File service registration
- Memory builder extensions
- Plugin collection extensions

---

## **Observability & Telemetry**

### ✅ **Implemented Observability (60% coverage)**
- OpenTelemetry activity source integration
- Request duration metrics
- Token usage tracking (normalized and native)
- Cost tracking from OpenRouter generation endpoint
- Request count and error metrics
- Activity tagging with model, operation type
- Real-time metrics collection (fire-and-forget)

### ❌ **Missing Observability (40% coverage)**
- Service-specific telemetry tags
- Model performance metrics
- Advanced diagnostic information
- Request/response payload logging
- Enhanced streaming telemetry
- Multi-service activity correlation

---

## **Error Handling**

### ✅ **Implemented Error Handling (50% coverage)**
- `OpenRouterException` with status codes and response content
- HTTP error response handling
- Comprehensive exception propagation
- Activity status tracking for errors
- Function invocation error handling

### ❌ **Missing Error Handling (50% coverage)**
- Rate limiting detection and retry logic
- Token limit exceeded specific handling
- Model-specific error responses
- Detailed error categorization
- Streaming error recovery mechanisms
- Sophisticated SDK error patterns

---

## **Multimodal Content Support**

### ✅ **Implemented Multimodal Features (90% coverage)**
- **Image Support** - PNG, JPEG, WebP format handling
- **PDF Processing** - Multiple processing engines (pdf-text, mistral-ocr, native)
- **File Support** - Document processing (PDF, Word, plain text)
- **Content Arrays** - Mixed content types in single messages
- **Base64 Encoding** - Automatic encoding utilities for file data
- **URL Support** - Direct image URL references
- **Processing Engine Selection** - Cost-optimized PDF processing options
- **Content Utilities** - Helper classes for easy content creation
- **JSON Serialization** - Custom polymorphic serialization for content arrays
- **Backward Compatibility** - Simple string content still supported

### ❌ **Missing Multimodal Features (10% coverage)**
- **Native SK Integration** - Currently uses reflection workaround for content types

### **Technical Implementation**

**Content Type Architecture:**
- `OpenRouterContentItem` - Abstract base class for all content types
- `OpenRouterTextContent` - Text content with type discriminator
- `OpenRouterImageContent` - Image content with URL and detail level support
- `OpenRouterFileContent` - File content with processing engine options

**Utilities Available:**
- `OpenRouterContentUtilities` - File encoding, MIME type detection, validation
- `OpenRouterContentHelper` - Simple content creation methods
- `OpenRouterContentItemConverter` - Custom JSON polymorphic serialization

**Reflection-based Conversion:**
Uses reflection to detect and convert Semantic Kernel content types (ImageContent, FileReferenceContent) to OpenRouter format, providing compatibility without direct dependencies.

---

## **Audio/Vision Support**

### ✅ **Implemented Multi-modal (50% coverage)**
- **Image input processing** - PNG, JPEG, WebP support
- **Vision/image understanding** - Via vision-capable models
- **Multi-modal conversations** - Text + images + documents

### ❌ **Missing Multi-modal (50% coverage)**
- Audio input processing (not supported by OpenRouter API)
- Audio output generation (not supported by OpenRouter API)
- Image output generation (not supported by OpenRouter API)
- Audio transcription capabilities (not supported by OpenRouter API)
- Speech synthesis (not supported by OpenRouter API)

---

## **Implementation Priorities**

### **Phase 1: High Impact** ✅
1. **~~Enhanced Configuration Options~~** - ✅ **COMPLETED** - Better model control and deterministic generation
2. **~~Text Embeddings Service~~** - ⚠️ **NOT SUPPORTED BY OPENROUTER API**

### **Phase 2: Medium Impact** 🟡
3. **Image Generation Service** - Visual AI capabilities
4. **Audio Services** - Voice AI capabilities (transcription + synthesis)

### **Phase 3: Nice to Have** 🟢
5. **Advanced Function Calling** - Parallel execution, enhanced validation
6. **Enhanced Streaming** - Better streaming experience with finish reasons
7. **File Operations** - File upload/download capabilities

---

## **Overall Feature Parity Score**

| Category | Coverage | Priority |
|----------|----------|----------|
| Core Services | 33% (2/6) | 🔴 High |
| Configuration Options | 85% | ✅ Completed |
| Content Types | 70% | ✅ Completed |
| Function Calling | 75% | 🟢 Low |
| Builder Extensions | 40% | 🟡 Medium |
| Observability | 60% | 🟢 Low |
| Error Handling | 50% | 🟡 Medium |
| Multi-modal Support | 90% | ✅ Completed |

**Overall Parity: ~75%**

---

## **API Limitations**

### **⚠️ OpenRouter API Constraints**

OpenRouter **does not support** the following endpoints that are available in OpenAI:
- `/v1/embeddings` - Text embeddings generation
- `/v1/images/generations` - Image generation (DALL-E)
- `/v1/audio/transcriptions` - Audio-to-text (Whisper)
- `/v1/audio/speech` - Text-to-audio
- `/v1/files` - File operations

**Available Endpoints:**
- ✅ `/v1/chat/completions` - Chat completions (implemented)
- ✅ `/v1/completions` - Text completions (implemented via chat)

---

## **Next Steps**

Given OpenRouter's API limitations, the immediate focus should be on:

### **1. ~~Enhanced Configuration Options~~** ✅
**COMPLETED** - Implemented comprehensive configuration options to maximize control over the supported endpoints:
- ✅ `Seed` for deterministic generation
- ✅ `ResponseFormat` for JSON mode (JsonObject and JsonSchema support)
- ✅ `LogitBias` for token control (with helper utilities)
- ✅ `User` for tracking
- ✅ `MaxCompletionTokens` for better token management
- ✅ `Store`, `Metadata`, `TopLogprobs`, `LogProbs`, `ServiceTier`, `ParallelToolCalls`

### **2. Hybrid Integration Guidance** 🟡
Provide documentation and examples for using OpenRouter alongside other providers:

```csharp
// Chat completions via OpenRouter
var kernel = Kernel.CreateBuilder()
    .AddOpenRouterChatCompletion(openRouterApiKey, "anthropic/claude-3-haiku")
    .AddOpenAITextEmbedding(openAiApiKey, "text-embedding-3-small")
    .Build();
```

### **3. Enhanced Streaming and Error Handling** 🟢
Improve the implemented services with:
- Better streaming with finish reasons
- Enhanced error handling and retry logic
- More sophisticated observability

This approach maximizes the value of OpenRouter's strengths (model variety, competitive pricing) while acknowledging its current API limitations.