using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using SemanticKernel.Connectors.OpenRouter.Services;
using System.Diagnostics.CodeAnalysis;

namespace SemanticKernel.Connectors.OpenRouter.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IKernelBuilder"/> to add OpenRouter services.
/// </summary>
[Experimental("SKEXP0001")]
public static class OpenRouterKernelBuilderExtensions
{
    /// <summary>
    /// Adds OpenRouter chat completion service to the kernel builder.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to configure.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for completions.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    public static IKernelBuilder AddOpenRouterChatCompletion(
        this IKernelBuilder builder,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds OpenRouter text generation service to the kernel builder.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to configure.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for text generation.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    public static IKernelBuilder AddOpenRouterTextGeneration(
        this IKernelBuilder builder,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        return builder;
    }

    /// <summary>
    /// Adds both OpenRouter chat completion and text generation services to the kernel builder.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to configure.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for completions.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    public static IKernelBuilder AddOpenRouter(
        this IKernelBuilder builder,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        // Register the service instance as both interfaces
        builder.Services.AddKeyedSingleton<OpenRouterChatCompletionService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        // Register as both chat completion and text generation services
        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, key) =>
            serviceProvider.GetRequiredKeyedService<OpenRouterChatCompletionService>(key));

        builder.Services.AddKeyedSingleton<ITextGenerationService>(serviceId, (serviceProvider, key) =>
            serviceProvider.GetRequiredKeyedService<OpenRouterChatCompletionService>(key));

        return builder;
    }
}

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add OpenRouter services.
/// </summary>
[Experimental("SKEXP0001")]
public static class OpenRouterServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenRouter chat completion service to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for completions.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddOpenRouterChatCompletion(
        this IServiceCollection services,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds OpenRouter text generation service to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for text generation.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddOpenRouterTextGeneration(
        this IServiceCollection services,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        services.AddKeyedSingleton<ITextGenerationService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds both OpenRouter chat completion and text generation services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="apiKey">The OpenRouter API key.</param>
    /// <param name="modelId">The model identifier to use for completions.</param>
    /// <param name="baseUrl">Optional base URL for the OpenRouter API.</param>
    /// <param name="serviceId">Optional unique identifier for the service.</param>
    /// <param name="httpClient">Optional HTTP client to use for requests.</param>
    /// <returns>The same instance as <paramref name="services"/>.</returns>
    public static IServiceCollection AddOpenRouter(
        this IServiceCollection services,
        string apiKey,
        string? modelId = null,
        Uri? baseUrl = null,
        string? serviceId = null,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        // Register the service instance
        services.AddKeyedSingleton<OpenRouterChatCompletionService>(serviceId, (serviceProvider, _) =>
        {
            var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<OpenRouterChatCompletionService>();
            return new OpenRouterChatCompletionService(apiKey, modelId, baseUrl, httpClient, logger);
        });

        // Register as both interfaces
        services.AddKeyedSingleton<IChatCompletionService>(serviceId, (serviceProvider, key) =>
            serviceProvider.GetRequiredKeyedService<OpenRouterChatCompletionService>(key));

        services.AddKeyedSingleton<ITextGenerationService>(serviceId, (serviceProvider, key) =>
            serviceProvider.GetRequiredKeyedService<OpenRouterChatCompletionService>(key));

        return services;
    }
}