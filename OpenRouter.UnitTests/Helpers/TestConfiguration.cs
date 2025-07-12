namespace OpenRouter.UnitTests.Helpers;

public static class TestConfiguration
{
    public static class OpenRouter
    {
        public static string ApiKey => Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "test-api-key";
        public static string ModelId => Environment.GetEnvironmentVariable("OPENROUTER_MODEL_ID") ?? "openai/gpt-3.5-turbo";
        public static string BaseUrl => Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1";
    }
}