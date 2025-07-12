using Microsoft.SemanticKernel;
using System.Diagnostics.CodeAnalysis;

namespace SemanticKernel.Connectors.OpenRouter.Exceptions;

[Experimental("SKEXP0001")]
public sealed class OpenRouterException : KernelException
{
    public OpenRouterException()
    {
    }

    public OpenRouterException(string? message) : base(message)
    {
    }

    public OpenRouterException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public string? ResponseContent { get; init; }

    public int? StatusCode { get; init; }
}