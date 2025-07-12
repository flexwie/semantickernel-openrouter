using Microsoft.SemanticKernel;
using SemanticKernel.Connectors.OpenRouter.Models;
using Xunit;

namespace OpenRouter.UnitTests.Models;

public class OpenRouterExecutionSettingsTests
{
    [Fact]
    public void FromExecutionSettings_WithNull_ReturnsNewInstance()
    {
        var result = OpenRouterExecutionSettings.FromExecutionSettings(null);
        
        Assert.NotNull(result);
        Assert.IsType<OpenRouterExecutionSettings>(result);
    }

    [Fact]
    public void FromExecutionSettings_WithOpenRouterSettings_ReturnsSameInstance()
    {
        var original = new OpenRouterExecutionSettings
        {
            ModelId = "test-model",
            Temperature = 0.8
        };

        var result = OpenRouterExecutionSettings.FromExecutionSettings(original);
        
        Assert.Same(original, result);
    }

    [Fact]
    public void FromExecutionSettings_WithGenericSettings_CreatesNewWithCopiedProperties()
    {
        var generic = new PromptExecutionSettings
        {
            ModelId = "test-model",
            ServiceId = "test-service",
            ExtensionData = new Dictionary<string, object> { ["custom"] = "value" }
        };

        var result = OpenRouterExecutionSettings.FromExecutionSettings(generic);
        
        Assert.NotNull(result);
        Assert.IsType<OpenRouterExecutionSettings>(result);
        Assert.Equal("test-model", result.ModelId);
        Assert.Equal("test-service", result.ServiceId);
        Assert.Equal("value", result.ExtensionData!["custom"]);
    }

    [Fact]
    public void Model_Property_SetsAndGetsModelId()
    {
        var settings = new OpenRouterExecutionSettings();
        
        settings.Model = "test-model";
        
        Assert.Equal("test-model", settings.ModelId);
        Assert.Equal("test-model", settings.Model);
    }

    [Fact]
    public void ModelId_Property_SetsAndGetsModel()
    {
        var settings = new OpenRouterExecutionSettings();
        
        settings.ModelId = "test-model";
        
        Assert.Equal("test-model", settings.Model);
        Assert.Equal("test-model", settings.ModelId);
    }

    [Fact]
    public void Properties_SetAndGetCorrectly()
    {
        var settings = new OpenRouterExecutionSettings
        {
            MaxTokens = 1000,
            Temperature = 0.7,
            TopP = 0.9,
            TopK = 50,
            FrequencyPenalty = 0.1,
            PresencePenalty = 0.2,
            RepetitionPenalty = 1.1,
            StopSequences = new[] { "stop1", "stop2" },
            Stream = true,
            Models = new[] { "model1", "model2" },
            Provider = new { test = "value" }
        };

        Assert.Equal(1000, settings.MaxTokens);
        Assert.Equal(0.7, settings.Temperature);
        Assert.Equal(0.9, settings.TopP);
        Assert.Equal(50, settings.TopK);
        Assert.Equal(0.1, settings.FrequencyPenalty);
        Assert.Equal(0.2, settings.PresencePenalty);
        Assert.Equal(1.1, settings.RepetitionPenalty);
        Assert.Equal(new[] { "stop1", "stop2" }, settings.StopSequences);
        Assert.True(settings.Stream);
        Assert.Equal(new[] { "model1", "model2" }, settings.Models);
        Assert.NotNull(settings.Provider);
    }

    [Fact]
    public void Properties_DefaultToNull()
    {
        var settings = new OpenRouterExecutionSettings();

        Assert.Null(settings.MaxTokens);
        Assert.Null(settings.Temperature);
        Assert.Null(settings.TopP);
        Assert.Null(settings.TopK);
        Assert.Null(settings.FrequencyPenalty);
        Assert.Null(settings.PresencePenalty);
        Assert.Null(settings.RepetitionPenalty);
        Assert.Null(settings.StopSequences);
        Assert.False(settings.Stream); // bool defaults to false
        Assert.Null(settings.Models);
        Assert.Null(settings.Provider);
    }

    [Fact]
    public void Stream_DefaultsToFalse()
    {
        var settings = new OpenRouterExecutionSettings();
        
        Assert.False(settings.Stream);
    }
}