using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextGeneration;
using SemanticKernel.Connectors.OpenRouter.Core;
using SemanticKernel.Connectors.OpenRouter.Models;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SemanticKernel.Connectors.OpenRouter.Services;

[Experimental("SKEXP0001")]
public sealed class OpenRouterChatCompletionService : IChatCompletionService, ITextGenerationService
{
    private readonly OpenRouterClient _client;
    private readonly ILogger _logger;

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    public OpenRouterChatCompletionService(
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        HttpClient? httpClient = null,
        ILogger<OpenRouterChatCompletionService>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OpenRouterChatCompletionService>.Instance;
        
        var client = httpClient ?? new HttpClient();
        _client = new OpenRouterClient(client, apiKey, baseUrl, _logger);

        var attributes = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(modelId))
        {
            attributes.Add(AIServiceExtensions.ModelIdKey, modelId);
        }
        Attributes = attributes;
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);

        // Check if we need to handle automatic function invocation
        if (kernel != null && 
            executionSettings?.FunctionChoiceBehavior != null && 
            ShouldAutoInvokeFunctions(executionSettings.FunctionChoiceBehavior))
        {
            return await OpenRouterFunctionInvoker.ProcessFunctionCallsAsync(
                this, chatHistory, executionSettings, kernel, _logger, cancellationToken);
        }

        return await GetChatMessageContentsInternalAsync(chatHistory, executionSettings, kernel, cancellationToken);
    }

    internal async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsInternalAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        var openRouterSettings = OpenRouterExecutionSettings.FromExecutionSettings(executionSettings);
        var request = CreateChatRequest(chatHistory, openRouterSettings, kernel, executionSettings);

        using var activity = OpenRouterTelemetry.ActivitySource.StartActivity(OpenRouterTelemetry.ActivityNames.ChatCompletion);
        var stopwatch = Stopwatch.StartNew();
        
        var tags = OpenRouterTelemetry.CreateTags(request.Model, OpenRouterTelemetry.OperationTypes.ChatCompletion, false);

        try
        {
            var response = await _client.GetChatCompletionAsync(request, cancellationToken);

            stopwatch.Stop();
            OpenRouterTelemetry.RecordDuration(OpenRouterTelemetry.OperationTypes.ChatCompletion, stopwatch.Elapsed.TotalSeconds, tags);
            OpenRouterTelemetry.RecordRequest(tags, false);

            // Record normalized token usage from immediate response
            if (response.Usage != null)
            {
                OpenRouterTelemetry.RecordTokenUsage(tags, response.Usage.PromptTokens, response.Usage.CompletionTokens, response.Usage.TotalTokens);
            }

            // Set activity attributes
            activity?.SetTag(OpenRouterTelemetry.TagNames.ModelId, response.Model);
            activity?.SetTag(OpenRouterTelemetry.TagNames.RequestId, response.Id);

            var results = new List<ChatMessageContent>();
            
            foreach (var choice in response.Choices)
            {
                var content = new ChatMessageContent(
                    role: ParseAuthorRole(choice.Message?.Role),
                    content: choice.Message?.Content,
                    modelId: response.Model,
                    innerContent: response,
                    metadata: CreateMetadata(response.Usage)
                );

                // Handle tool calls
                if (choice.Message?.ToolCalls != null)
                {
                    foreach (var toolCall in choice.Message.ToolCalls)
                    {
                        content.Items.Add(CreateFunctionCallContent(toolCall));
                    }
                }

                results.Add(content);
            }

            return results;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            OpenRouterTelemetry.RecordRequest(tags, true);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);

        var openRouterSettings = OpenRouterExecutionSettings.FromExecutionSettings(executionSettings);
        var request = CreateChatRequest(chatHistory, openRouterSettings, kernel, executionSettings);

        var streamEnum = GetStreamingChatCompletionCoreAsync(request, cancellationToken);
        
        await foreach (var result in streamEnum)
        {
            yield return result;
        }
    }

    private async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatCompletionCoreAsync(
        OpenRouterRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var activity = OpenRouterTelemetry.ActivitySource.StartActivity(OpenRouterTelemetry.ActivityNames.StreamingChatCompletion);
        var stopwatch = Stopwatch.StartNew();
        
        var tags = OpenRouterTelemetry.CreateTags(request.Model, OpenRouterTelemetry.OperationTypes.ChatCompletion, true);

        await foreach (var streamResponse in _client.GetStreamingChatCompletionAsync(request, cancellationToken))
        {
            // Set activity attributes from first response
            if (activity != null && !string.IsNullOrEmpty(streamResponse.Id))
            {
                activity.SetTag(OpenRouterTelemetry.TagNames.ModelId, streamResponse.Model);
                activity.SetTag(OpenRouterTelemetry.TagNames.RequestId, streamResponse.Id);
            }

            foreach (var choice in streamResponse.Choices)
            {
                var streamingContent = new StreamingChatMessageContent(
                    role: ParseAuthorRole(choice.Delta?.Role),
                    content: choice.Delta?.Content,
                    innerContent: streamResponse,
                    choiceIndex: choice.Index,
                    modelId: streamResponse.Model
                );

                // Handle streaming tool calls
                if (choice.Delta?.ToolCalls != null)
                {
                    foreach (var toolCall in choice.Delta.ToolCalls)
                    {
                        // For streaming, we'll include the function call content in a different way
                        // since Items collection expects StreamingKernelContent
                        var functionCallContent = CreateFunctionCallContent(toolCall);
                        // TODO: Handle streaming function calls properly - for now skip
                    }
                }

                yield return streamingContent;
            }
        }

        stopwatch.Stop();
        OpenRouterTelemetry.RecordDuration(OpenRouterTelemetry.OperationTypes.ChatCompletion, stopwatch.Elapsed.TotalSeconds, tags);
        OpenRouterTelemetry.RecordRequest(tags, false);
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        using var activity = OpenRouterTelemetry.ActivitySource.StartActivity(OpenRouterTelemetry.ActivityNames.TextGeneration);
        var stopwatch = Stopwatch.StartNew();
        
        var modelId = OpenRouterExecutionSettings.FromExecutionSettings(executionSettings).ModelId ?? GetModelIdFromAttributes();
        var tags = OpenRouterTelemetry.CreateTags(modelId ?? "unknown", OpenRouterTelemetry.OperationTypes.TextGeneration, false);

        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var chatResults = await GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);

            stopwatch.Stop();
            OpenRouterTelemetry.RecordDuration(OpenRouterTelemetry.OperationTypes.TextGeneration, stopwatch.Elapsed.TotalSeconds, tags);

            // Set activity attributes
            if (chatResults.Any())
            {
                activity?.SetTag(OpenRouterTelemetry.TagNames.ModelId, chatResults[0].ModelId);
            }

            return chatResults.Select(chat => new TextContent(
                text: chat.Content,
                modelId: chat.ModelId,
                innerContent: chat.InnerContent,
                metadata: chat.Metadata
            )).ToList();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt, 
        PromptExecutionSettings? executionSettings = null, 
        Kernel? kernel = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);

        await foreach (var streamingChat in GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken))
        {
            yield return new StreamingTextContent(
                text: streamingChat.Content,
                modelId: streamingChat.ModelId,
                innerContent: streamingChat.InnerContent,
                choiceIndex: streamingChat.ChoiceIndex
            );
        }
    }

    private OpenRouterRequest CreateChatRequest(
        ChatHistory chatHistory, 
        OpenRouterExecutionSettings settings, 
        Kernel? kernel = null, 
        PromptExecutionSettings? executionSettings = null)
    {
        var modelId = settings.ModelId ?? GetModelIdFromAttributes();
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("Model ID must be specified either in settings or service attributes");
        }

        var request = new OpenRouterRequest
        {
            Model = modelId,
            Messages = chatHistory.Select(CreateOpenRouterMessage).ToArray(),
            MaxTokens = settings.MaxTokens,
            Temperature = settings.Temperature,
            TopP = settings.TopP,
            TopK = settings.TopK,
            FrequencyPenalty = settings.FrequencyPenalty,
            PresencePenalty = settings.PresencePenalty,
            RepetitionPenalty = settings.RepetitionPenalty,
            StopSequences = settings.StopSequences,
            Models = settings.Models,
            Provider = settings.Provider
        };

        // Handle function calling
        if (kernel != null && executionSettings is { FunctionChoiceBehavior: not null })
        {
            var functions = GetEnabledFunctions(executionSettings.FunctionChoiceBehavior, kernel.Plugins);
            if (functions.Any())
            {
                request.Tools = OpenRouterFunctionHelpers.ConvertToOpenRouterTools(functions);
                request.ToolChoice = OpenRouterFunctionHelpers.ConvertFunctionChoiceBehaviorToToolChoice(
                    executionSettings.FunctionChoiceBehavior, functions);
            }
        }

        return request;
    }

    private static OpenRouterMessage CreateOpenRouterMessage(ChatMessageContent message)
    {
        var openRouterMessage = new OpenRouterMessage
        {
            Role = ConvertRole(message.Role),
            Content = message.Content,
            Name = GetMessageName(message)
        };

        // Handle function calls in assistant messages
        var functionCalls = message.Items.OfType<FunctionCallContent>().ToArray();
        if (functionCalls.Length > 0)
        {
            openRouterMessage.ToolCalls = functionCalls.Select(CreateOpenRouterToolCall).ToArray();
        }

        // Handle function results in tool messages
        var functionResult = message.Items.OfType<FunctionResultContent>().FirstOrDefault();
        if (functionResult != null)
        {
            openRouterMessage.ToolCallId = functionResult.CallId;
            openRouterMessage.Content = functionResult.Result?.ToString();
        }

        return openRouterMessage;
    }

    private static string ConvertRole(AuthorRole role)
    {
        if (role == AuthorRole.User) return "user";
        if (role == AuthorRole.Assistant) return "assistant";
        if (role == AuthorRole.System) return "system";
        if (role == AuthorRole.Tool) return "tool";
        
        return "user";
    }

    private static string? GetMessageName(ChatMessageContent message)
    {
        return message.AuthorName;
    }

    private static AuthorRole ParseAuthorRole(string? role)
    {
        if (role == "user") return AuthorRole.User;
        if (role == "assistant") return AuthorRole.Assistant;
        if (role == "system") return AuthorRole.System;
        if (role == "tool") return AuthorRole.Tool;
        
        return AuthorRole.Assistant;
    }

    private string? GetModelIdFromAttributes()
    {
        return Attributes.TryGetValue(AIServiceExtensions.ModelIdKey, out var modelId) 
            ? modelId as string 
            : null;
    }

    private static Dictionary<string, object?> CreateMetadata(OpenRouterUsage? usage)
    {
        var metadata = new Dictionary<string, object?>();
        
        if (usage != null)
        {
            metadata["Usage"] = usage;
            metadata["PromptTokens"] = usage.PromptTokens;
            metadata["CompletionTokens"] = usage.CompletionTokens;
            metadata["TotalTokens"] = usage.TotalTokens;
        }

        return metadata;
    }

    private static FunctionCallContent CreateFunctionCallContent(OpenRouterToolCall toolCall)
    {
        var (pluginName, functionName) = OpenRouterFunctionHelpers.ParseFunctionName(toolCall.Function.Name);
        
        // Parse arguments JSON
        object? arguments = null;
        if (!string.IsNullOrEmpty(toolCall.Function.Arguments))
        {
            try
            {
                arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(toolCall.Function.Arguments);
            }
            catch (JsonException)
            {
                // If parsing fails, use the raw string
                arguments = toolCall.Function.Arguments;
            }
        }

        return new FunctionCallContent(
            functionName: functionName,
            pluginName: pluginName,
            id: toolCall.Id,
            arguments: arguments as KernelArguments ?? new KernelArguments());
    }

    private static OpenRouterToolCall CreateOpenRouterToolCall(FunctionCallContent functionCall)
    {
        // Create function name by combining plugin and function names
        var functionName = string.IsNullOrEmpty(functionCall.PluginName) 
            ? functionCall.FunctionName 
            : $"{functionCall.PluginName}-{functionCall.FunctionName}";

        return new OpenRouterToolCall
        {
            Id = functionCall.Id ?? Guid.NewGuid().ToString(),
            Type = "function",
            Function = new OpenRouterFunctionCall
            {
                Name = functionName,
                Arguments = JsonSerializer.Serialize(functionCall.Arguments)
            }
        };
    }

    /// <summary>
    /// Determines if functions should be auto-invoked based on the function choice behavior.
    /// </summary>
    /// <param name="functionChoiceBehavior">The function choice behavior.</param>
    /// <returns>True if functions should be auto-invoked; otherwise, false.</returns>
    private static bool ShouldAutoInvokeFunctions(FunctionChoiceBehavior functionChoiceBehavior)
    {
        // Use reflection to access the AutoInvoke property
        var autoInvokeProperty = functionChoiceBehavior.GetType().GetProperty("AutoInvoke");
        if (autoInvokeProperty?.GetValue(functionChoiceBehavior) is bool autoInvoke)
        {
            return autoInvoke;
        }

        // Default to false if property is not found
        return false;
    }

    /// <summary>
    /// Gets the enabled functions from the function choice behavior.
    /// </summary>
    /// <param name="functionChoiceBehavior">The function choice behavior.</param>
    /// <param name="plugins">The kernel plugins.</param>
    /// <returns>The enabled functions.</returns>
    private static IEnumerable<KernelFunction> GetEnabledFunctions(
        FunctionChoiceBehavior functionChoiceBehavior, 
        KernelPluginCollection plugins)
    {
        // For simplified implementation, return all functions from all plugins
        // TODO: Implement proper function filtering based on behavior type
        return plugins.SelectMany(plugin => plugin);
    }
}