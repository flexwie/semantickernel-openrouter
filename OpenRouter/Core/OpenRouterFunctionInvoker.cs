using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SemanticKernel.Connectors.OpenRouter.Core;

/// <summary>
/// Handles automatic function invocation for OpenRouter chat completion.
/// </summary>
internal static class OpenRouterFunctionInvoker
{
    /// <summary>
    /// Maximum number of function invocation iterations to prevent infinite loops.
    /// </summary>
    private const int MaxIterations = 128;

    /// <summary>
    /// Processes function calls and auto-invokes them if specified in the function choice behavior.
    /// </summary>
    /// <param name="service">The OpenRouter chat completion service.</param>
    /// <param name="chatHistory">The chat history.</param>
    /// <param name="executionSettings">The execution settings.</param>
    /// <param name="kernel">The kernel instance.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The final chat message contents after all function calls are processed.</returns>
    public static async Task<IReadOnlyList<ChatMessageContent>> ProcessFunctionCallsAsync(
        Services.OpenRouterChatCompletionService service,
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings,
        Kernel? kernel,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (kernel == null || executionSettings?.FunctionChoiceBehavior == null)
        {
            // No function calling - use regular chat completion
            return await service.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        }

        var functionChoiceBehavior = executionSettings.FunctionChoiceBehavior;
        var shouldAutoInvoke = GetAutoInvokeFromBehavior(functionChoiceBehavior);
        var maxIterations = GetMaxIterationsFromBehavior(functionChoiceBehavior) ?? MaxIterations;

        var workingHistory = new ChatHistory(chatHistory);
        var allResults = new List<ChatMessageContent>();
        var iterationCount = 0;

        while (iterationCount < maxIterations)
        {
            iterationCount++;

            // Get response from OpenRouter (use internal method to avoid recursion)
            var results = await service.GetChatMessageContentsInternalAsync(workingHistory, executionSettings, kernel, cancellationToken);
            allResults.AddRange(results);

            var result = results.FirstOrDefault();
            if (result == null)
            {
                break;
            }

            // Add assistant response to working history
            workingHistory.Add(result);

            // Check for function calls
            var functionCalls = result.Items.OfType<FunctionCallContent>().ToArray();
            if (functionCalls.Length == 0)
            {
                // No function calls - we're done
                break;
            }

            if (!shouldAutoInvoke)
            {
                // Function calls present but auto-invoke is disabled - return as is
                break;
            }

            // Auto-invoke functions
            var functionResults = await InvokeFunctionsAsync(functionCalls, kernel, logger, cancellationToken);
            
            // Add function results to working history
            foreach (var functionResult in functionResults)
            {
                var toolMessage = new ChatMessageContent(AuthorRole.Tool, functionResult.Result?.ToString());
                toolMessage.Items.Add(functionResult);
                workingHistory.Add(toolMessage);
            }

            allResults.AddRange(functionResults.Select(fr => 
            {
                var toolMessage = new ChatMessageContent(AuthorRole.Tool, fr.Result?.ToString());
                toolMessage.Items.Add(fr);
                return toolMessage;
            }));
        }

        if (iterationCount >= maxIterations)
        {
            logger.LogWarning(
                "Maximum function calling iterations ({MaxIterations}) reached. Stopping function invocation.",
                maxIterations);
        }

        return allResults;
    }

    /// <summary>
    /// Invokes multiple functions in parallel or sequentially based on configuration.
    /// </summary>
    /// <param name="functionCalls">The function calls to invoke.</param>
    /// <param name="kernel">The kernel instance.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The function results.</returns>
    private static async Task<List<FunctionResultContent>> InvokeFunctionsAsync(
        FunctionCallContent[] functionCalls,
        Kernel kernel,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var results = new List<FunctionResultContent>();

        // For now, invoke functions sequentially
        // TODO: Add parallel invocation support if needed
        foreach (var functionCall in functionCalls)
        {
            try
            {
                using var activity = OpenRouterTelemetry.ActivitySource.StartActivity($"OpenRouter.InvokeFunction.{functionCall.FunctionName}");
                activity?.SetTag("function.name", functionCall.FunctionName);
                activity?.SetTag("function.plugin", functionCall.PluginName);

                var functionResult = await functionCall.InvokeAsync(kernel, cancellationToken);

                results.Add(new FunctionResultContent(
                    functionName: functionCall.FunctionName,
                    pluginName: functionCall.PluginName,
                    callId: functionCall.Id,
                    result: functionResult.ToString()));

                logger.LogDebug(
                    "Function {PluginName}.{FunctionName} invoked successfully with result: {Result}",
                    functionCall.PluginName,
                    functionCall.FunctionName,
                    functionResult.ToString());
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Error invoking function {PluginName}.{FunctionName}: {Error}",
                    functionCall.PluginName,
                    functionCall.FunctionName,
                    ex.Message);

                results.Add(new FunctionResultContent(
                    functionName: functionCall.FunctionName,
                    pluginName: functionCall.PluginName,
                    callId: functionCall.Id,
                    result: $"Error: {ex.Message}"));
            }
        }

        return results;
    }

    /// <summary>
    /// Extracts the auto-invoke setting from function choice behavior using reflection.
    /// </summary>
    /// <param name="functionChoiceBehavior">The function choice behavior.</param>
    /// <returns>True if auto-invoke is enabled; otherwise, false.</returns>
    private static bool GetAutoInvokeFromBehavior(FunctionChoiceBehavior functionChoiceBehavior)
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
    /// Extracts the maximum iterations setting from function choice behavior using reflection.
    /// </summary>
    /// <param name="functionChoiceBehavior">The function choice behavior.</param>
    /// <returns>The maximum iterations if found; otherwise, null.</returns>
    private static int? GetMaxIterationsFromBehavior(FunctionChoiceBehavior functionChoiceBehavior)
    {
        // Use reflection to access the MaximumAutoInvokeAttempts property
        var maxIterationsProperty = functionChoiceBehavior.GetType().GetProperty("MaximumAutoInvokeAttempts");
        if (maxIterationsProperty?.GetValue(functionChoiceBehavior) is int maxIterations)
        {
            return maxIterations;
        }

        // Return null if property is not found - caller will use default
        return null;
    }
}