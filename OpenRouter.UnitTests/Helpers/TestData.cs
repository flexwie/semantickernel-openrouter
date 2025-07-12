namespace OpenRouter.UnitTests.Helpers;

public static class TestData
{
    public const string ChatCompletionResponse = """
    {
        "id": "chatcmpl-test123",
        "object": "chat.completion",
        "created": 1677652288,
        "model": "openai/gpt-3.5-turbo",
        "choices": [
            {
                "index": 0,
                "message": {
                    "role": "assistant",
                    "content": "Hello! How can I help you today?"
                },
                "finish_reason": "stop"
            }
        ],
        "usage": {
            "prompt_tokens": 12,
            "completion_tokens": 10,
            "total_tokens": 22
        }
    }
    """;

    public const string StreamingChatCompletionChunk1 = """
    data: {"id":"chatcmpl-test123","object":"chat.completion.chunk","created":1677652288,"model":"openai/gpt-3.5-turbo","choices":[{"index":0,"delta":{"role":"assistant","content":"Hello"},"finish_reason":null}]}

    """;

    public const string StreamingChatCompletionChunk2 = """
    data: {"id":"chatcmpl-test123","object":"chat.completion.chunk","created":1677652288,"model":"openai/gpt-3.5-turbo","choices":[{"index":0,"delta":{"content":"! How can I help you today?"},"finish_reason":null}]}

    """;

    public const string StreamingChatCompletionFinalChunk = """
    data: {"id":"chatcmpl-test123","object":"chat.completion.chunk","created":1677652288,"model":"openai/gpt-3.5-turbo","choices":[{"index":0,"delta":{},"finish_reason":"stop"}]}

    """;

    public const string StreamingDoneMarker = "data: [DONE]\n\n";

    public const string ErrorResponse = """
    {
        "error": {
            "message": "Invalid API key",
            "type": "invalid_request_error",
            "code": "invalid_api_key"
        }
    }
    """;

    public static string GetStreamingResponse() =>
        StreamingChatCompletionChunk1 + StreamingChatCompletionChunk2 + StreamingChatCompletionFinalChunk + StreamingDoneMarker;
}