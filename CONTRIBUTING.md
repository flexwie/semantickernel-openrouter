# Contributing to SemanticKernel.Connectors.OpenRouter

Thank you for your interest in contributing to the OpenRouter connector for Microsoft Semantic Kernel!

## Getting Started

1. **Fork the repository** on GitHub
2. **Clone your fork** locally
3. **Create a branch** for your changes
4. **Make your changes** and commit them
5. **Push to your fork** and submit a pull request

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, JetBrains Rider)
- Git

### Building the Project
```bash
# Clone the repository
git clone https://github.com/yourusername/SemanticKernel.OpenRouter.git
cd SemanticKernel.OpenRouter

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## Coding Standards

### Code Style
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and reasonably sized

### Testing
- **Write unit tests** for all new functionality
- **Maintain test coverage** above 90%
- **Use descriptive test names** that explain what is being tested
- **Follow the AAA pattern** (Arrange, Act, Assert)
- **Mock external dependencies** (HTTP calls, etc.)

### Example Test Structure
```csharp
[Fact]
public void GetChatMessageContentsAsync_WithValidInput_ReturnsExpectedResponse()
{
    // Arrange
    var mockHandler = new MockHttpMessageHandler(HttpStatusCode.OK, testResponse);
    var service = new OpenRouterChatCompletionService("test-key", "test-model", httpClient: new HttpClient(mockHandler));
    
    // Act
    var result = await service.GetChatMessageContentsAsync(chatHistory);
    
    // Assert
    Assert.NotNull(result);
    Assert.Single(result);
    Assert.Equal("Expected content", result[0].Content);
}
```

## Pull Request Guidelines

### Before Submitting
- [ ] **Run all tests** and ensure they pass
- [ ] **Add tests** for new functionality
- [ ] **Update documentation** if needed
- [ ] **Follow the coding standards** outlined above
- [ ] **Keep commits atomic** - one logical change per commit
- [ ] **Write descriptive commit messages**

### Commit Message Format
```
type: brief description

More detailed explanation if needed.

- Key change 1
- Key change 2
```

**Types:**
- `feat:` new feature
- `fix:` bug fix
- `docs:` documentation changes
- `test:` adding or updating tests
- `chore:` maintenance tasks
- `refactor:` code refactoring

### Pull Request Process
1. **Create a descriptive title** for your PR
2. **Fill out the PR template** completely
3. **Link any related issues** in the description
4. **Ensure CI checks pass** before requesting review
5. **Be responsive** to feedback and review comments

## Issue Guidelines

### Reporting Bugs
- Use the **bug report template**
- Provide a **minimal code sample** that reproduces the issue
- Include **environment details** (OS, .NET version, package version)
- Add **full error messages** and stack traces

### Requesting Features
- Use the **feature request template**
- Explain the **use case** and **business value**
- Provide **example usage** of the proposed feature
- Consider **alternative approaches**

## Development Guidelines

### Adding New Features
1. **Check existing issues** to avoid duplication
2. **Create an issue** to discuss the feature before implementation
3. **Follow OpenRouter API patterns** when adding new functionality
4. **Add comprehensive tests** including edge cases
5. **Update documentation** and examples

### Modifying Existing Features
1. **Ensure backward compatibility** unless it's a breaking change
2. **Update existing tests** to reflect changes
3. **Add new tests** for modified behavior
4. **Update documentation** if the API changes

### OpenRouter API Integration
- **Test with real API** when possible (use your own API key)
- **Verify with multiple models** to ensure broad compatibility
- **Check cost and token metrics** accuracy
- **Test both streaming and non-streaming** scenarios

## Code Review Process

### For Contributors
- **Be patient** - reviews take time
- **Be responsive** to feedback
- **Ask questions** if feedback is unclear
- **Make suggested changes** promptly

### For Reviewers
- **Be constructive** and helpful
- **Focus on code quality** and maintainability
- **Test the changes** locally if possible
- **Approve when satisfied** with the changes

## Security

### Reporting Security Issues
- **Do not** create public issues for security vulnerabilities
- **Email** security concerns to the maintainers
- **Provide details** about the vulnerability and potential impact

### Security Best Practices
- **Never commit** API keys or secrets
- **Validate inputs** to prevent injection attacks
- **Use secure HTTP** for all API communications
- **Follow** Microsoft's security guidelines

## Getting Help

### Communication Channels
- **GitHub Issues** - for bugs and feature requests
- **GitHub Discussions** - for questions and community discussion
- **Pull Request Comments** - for code-specific questions

### Resources
- [Microsoft Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
- [OpenRouter API Documentation](https://openrouter.ai/docs)
- [.NET API Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

## License

By contributing to this project, you agree that your contributions will be licensed under the [MIT License](LICENSE).

## Recognition

Contributors will be recognized in:
- **Release notes** for significant contributions
- **README.md** contributors section
- **GitHub contributors** graph

Thank you for helping make the OpenRouter connector better! 🚀