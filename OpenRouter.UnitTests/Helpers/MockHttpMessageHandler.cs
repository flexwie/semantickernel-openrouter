using System.Net;
using System.Text;

namespace OpenRouter.UnitTests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;
    private readonly Exception? _exception;

    public List<HttpRequestMessage> Requests { get; } = new();

    public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
    }

    public MockHttpMessageHandler(Exception exception)
    {
        _exception = exception;
        _statusCode = HttpStatusCode.InternalServerError;
        _responseContent = string.Empty;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_exception != null)
        {
            throw _exception;
        }

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
        };

        return await Task.FromResult(response);
    }

    public HttpRequestMessage GetLastRequest() => Requests.LastOrDefault() ?? throw new InvalidOperationException("No requests captured");

    public async Task<string> GetLastRequestContentAsync()
    {
        var lastRequest = GetLastRequest();
        if (lastRequest.Content == null)
            return string.Empty;

        return await lastRequest.Content.ReadAsStringAsync();
    }
}