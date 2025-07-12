using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using SemanticKernel.Connectors.OpenRouter.Extensions;
using SemanticKernel.Connectors.OpenRouter.Services;
using Xunit;

namespace OpenRouter.UnitTests.Extensions;

public class OpenRouterKernelBuilderExtensionsTests
{
    [Fact]
    public void AddOpenRouterChatCompletion_WithValidParameters_RegistersService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        builder.AddOpenRouterChatCompletion("test-api-key", "test-model");
        var kernel = builder.Build();

        // Assert
        var service = kernel.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouterChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var serviceId = "test-service";

        // Act
        builder.AddOpenRouterChatCompletion("test-api-key", "test-model", serviceId: serviceId);
        var kernel = builder.Build();

        // Assert
        var service = kernel.Services.GetKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouterChatCompletion_WithNullApiKey_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder.AddOpenRouterChatCompletion(null!, "test-model"));
    }

    [Fact]
    public void AddOpenRouterChatCompletion_WithEmptyApiKey_ThrowsArgumentException()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            builder.AddOpenRouterChatCompletion("", "test-model"));
    }

    [Fact]
    public void AddOpenRouterTextGeneration_WithValidParameters_RegistersService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        builder.AddOpenRouterTextGeneration("test-api-key", "test-model");
        var kernel = builder.Build();

        // Assert
        var service = kernel.GetRequiredService<ITextGenerationService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouterTextGeneration_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var serviceId = "test-service";

        // Act
        builder.AddOpenRouterTextGeneration("test-api-key", "test-model", serviceId: serviceId);
        var kernel = builder.Build();

        // Assert
        var service = kernel.Services.GetKeyedService<ITextGenerationService>(serviceId);
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouter_WithValidParameters_RegistersBothServices()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        builder.AddOpenRouter("test-api-key", "test-model");
        var kernel = builder.Build();

        // Assert
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var textService = kernel.GetRequiredService<ITextGenerationService>();
        
        Assert.NotNull(chatService);
        Assert.NotNull(textService);
        Assert.IsType<OpenRouterChatCompletionService>(chatService);
        Assert.IsType<OpenRouterChatCompletionService>(textService);
        
        // Should be the same instance
        Assert.Same(chatService, textService);
    }

    [Fact]
    public void AddOpenRouter_WithServiceId_RegistersKeyedServices()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var serviceId = "test-service";

        // Act
        builder.AddOpenRouter("test-api-key", "test-model", serviceId: serviceId);
        var kernel = builder.Build();

        // Assert
        var chatService = kernel.Services.GetKeyedService<IChatCompletionService>(serviceId);
        var textService = kernel.Services.GetKeyedService<ITextGenerationService>(serviceId);
        
        Assert.NotNull(chatService);
        Assert.NotNull(textService);
        Assert.IsType<OpenRouterChatCompletionService>(chatService);
        Assert.IsType<OpenRouterChatCompletionService>(textService);
        
        // Should be the same instance
        Assert.Same(chatService, textService);
    }

    [Fact]
    public void AddOpenRouter_WithCustomBaseUrl_PassesBaseUrlToService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var customBaseUrl = new Uri("https://custom.openrouter.com/v1");

        // Act
        builder.AddOpenRouter("test-api-key", "test-model", baseUrl: customBaseUrl);
        var kernel = builder.Build();

        // Assert
        var service = kernel.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouter_WithCustomHttpClient_PassesHttpClientToService()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        var httpClient = new HttpClient();

        // Act
        builder.AddOpenRouter("test-api-key", "test-model", httpClient: httpClient);
        var kernel = builder.Build();

        // Assert
        var service = kernel.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouter_WithNullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IKernelBuilder? builder = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            builder!.AddOpenRouter("test-api-key", "test-model"));
    }

    [Fact]
    public void AddOpenRouter_MultipleServices_RegistersMultipleInstances()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        builder.AddOpenRouter("test-api-key-1", "test-model-1", serviceId: "service-1");
        builder.AddOpenRouter("test-api-key-2", "test-model-2", serviceId: "service-2");
        var kernel = builder.Build();

        // Assert
        var service1 = kernel.Services.GetKeyedService<IChatCompletionService>("service-1");
        var service2 = kernel.Services.GetKeyedService<IChatCompletionService>("service-2");
        
        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotSame(service1, service2);
    }

    [Fact]
    public void AddOpenRouter_WithoutModelId_RegistersServiceSuccessfully()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        builder.AddOpenRouter("test-api-key");
        var kernel = builder.Build();

        // Assert
        var service = kernel.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }
}

public class OpenRouterServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenRouterChatCompletion_WithValidParameters_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenRouterChatCompletion("test-api-key", "test-model");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouterTextGeneration_WithValidParameters_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenRouterTextGeneration("test-api-key", "test-model");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = serviceProvider.GetRequiredService<ITextGenerationService>();
        Assert.NotNull(service);
        Assert.IsType<OpenRouterChatCompletionService>(service);
    }

    [Fact]
    public void AddOpenRouter_WithValidParameters_RegistersBothServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenRouter("test-api-key", "test-model");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        var textService = serviceProvider.GetRequiredService<ITextGenerationService>();
        
        Assert.NotNull(chatService);
        Assert.NotNull(textService);
        Assert.Same(chatService, textService);
    }

    [Fact]
    public void AddOpenRouter_WithServiceId_RegistersKeyedServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceId = "test-service";

        // Act
        services.AddOpenRouter("test-api-key", "test-model", serviceId: serviceId);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
        var textService = serviceProvider.GetKeyedService<ITextGenerationService>(serviceId);
        
        Assert.NotNull(chatService);
        Assert.NotNull(textService);
        Assert.Same(chatService, textService);
    }

    [Fact]
    public void AddOpenRouter_WithNullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services!.AddOpenRouter("test-api-key", "test-model"));
    }
}