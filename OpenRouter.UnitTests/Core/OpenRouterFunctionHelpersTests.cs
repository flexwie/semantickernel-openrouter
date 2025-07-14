using Microsoft.SemanticKernel;
using SemanticKernel.Connectors.OpenRouter.Core;
using System.ComponentModel;
using Xunit;

namespace OpenRouter.UnitTests.Core;

public class TestPlugin
{
    [KernelFunction, Description("A test function")]
    public string TestFunction() => "test";

    [KernelFunction, Description("Function with parameters")]
    public string RepeatText([Description("Input text")] string input, [Description("Number of times")] int count) 
        => $"{input} repeated {count} times";
}

public class OpenRouterFunctionHelpersTests
{
    [Fact]
    public void CreateFunctionName_WithPluginName_CombinesCorrectly()
    {
        // Arrange
        var plugin = KernelPluginFactory.CreateFromObject(new TestPlugin(), "TestPlugin");
        var function = plugin["TestFunction"];

        // Act
        var result = OpenRouterFunctionHelpers.CreateFunctionName(function);

        // Assert
        Assert.Equal("TestPlugin-TestFunction", result);
    }

    [Fact]
    public void CreateFunctionName_WithoutPluginName_ReturnsFunction()
    {
        // Arrange
        var function = KernelFunctionFactory.CreateFromMethod(() => "test", "TestFunction", "Test description");

        // Act
        var result = OpenRouterFunctionHelpers.CreateFunctionName(function);

        // Assert
        Assert.Equal("TestFunction", result);
    }

    [Fact]
    public void ParseFunctionName_WithSeparator_ParsesCorrectly()
    {
        // Arrange
        var functionName = "TestPlugin-TestFunction";

        // Act
        var (pluginName, function) = OpenRouterFunctionHelpers.ParseFunctionName(functionName);

        // Assert
        Assert.Equal("TestPlugin", pluginName);
        Assert.Equal("TestFunction", function);
    }

    [Fact]
    public void ParseFunctionName_WithoutSeparator_ReturnsNullPlugin()
    {
        // Arrange
        var functionName = "TestFunction";

        // Act
        var (pluginName, function) = OpenRouterFunctionHelpers.ParseFunctionName(functionName);

        // Assert
        Assert.Null(pluginName);
        Assert.Equal("TestFunction", function);
    }

    [Fact]
    public void ConvertToOpenRouterFunction_WithParameters_CreatesCorrectSchema()
    {
        // Arrange
        var plugin = KernelPluginFactory.CreateFromObject(new TestPlugin(), "TestPlugin");
        var function = plugin["RepeatText"];

        // Act
        var result = OpenRouterFunctionHelpers.ConvertToOpenRouterFunction(function);

        // Assert
        Assert.Equal("TestPlugin-RepeatText", result.Name);
        Assert.Equal("Function with parameters", result.Description);
        Assert.NotNull(result.Parameters);
        Assert.Equal("object", result.Parameters.Type);
        Assert.Contains("input", result.Parameters.Properties.Keys);
        Assert.Contains("count", result.Parameters.Properties.Keys);
        Assert.Equal("string", result.Parameters.Properties["input"].Type);
        Assert.Equal("integer", result.Parameters.Properties["count"].Type);
        Assert.Contains("input", result.Parameters.Required);
        Assert.Contains("count", result.Parameters.Required);
    }

    [Fact]
    public void ConvertToOpenRouterTool_CreatesCorrectStructure()
    {
        // Arrange
        var plugin = KernelPluginFactory.CreateFromObject(new TestPlugin(), "TestPlugin");
        var function = plugin["TestFunction"];

        // Act
        var result = OpenRouterFunctionHelpers.ConvertToOpenRouterTool(function);

        // Assert
        Assert.Equal("function", result.Type);
        Assert.NotNull(result.Function);
        Assert.Equal("TestPlugin-TestFunction", result.Function.Name);
        Assert.Equal("A test function", result.Function.Description);
    }

    [Fact]
    public void ConvertFunctionChoiceBehaviorToToolChoice_WithAuto_ReturnsAuto()
    {
        // Arrange
        var behavior = FunctionChoiceBehavior.Auto();

        // Act
        var result = OpenRouterFunctionHelpers.ConvertFunctionChoiceBehaviorToToolChoice(behavior);

        // Assert
        Assert.Equal("auto", result);
    }

    [Fact]
    public void ConvertFunctionChoiceBehaviorToToolChoice_WithNone_ReturnsNone()
    {
        // Arrange
        var behavior = FunctionChoiceBehavior.None();

        // Act
        var result = OpenRouterFunctionHelpers.ConvertFunctionChoiceBehaviorToToolChoice(behavior);

        // Assert
        Assert.Equal("none", result);
    }

    [Fact]
    public void ConvertFunctionChoiceBehaviorToToolChoice_WithNull_ReturnsNull()
    {
        // Act
        var result = OpenRouterFunctionHelpers.ConvertFunctionChoiceBehaviorToToolChoice(null);

        // Assert
        Assert.Null(result);
    }
}