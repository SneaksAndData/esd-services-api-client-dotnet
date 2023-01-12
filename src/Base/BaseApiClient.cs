using System.Net;
using System.Net.Http.Headers;
using ESD.ApiClient.Boxer.Base;
using Microsoft.Extensions.Logging;
using Polly;

namespace ESD.ApiClient.Base;

/// <summary>
/// Base class for all API clients
/// </summary>
public abstract class BaseApiClient
{
    private readonly HttpClient httpClient;
    private readonly IBoxerTokenProvider boxerTokenProvider;
    private readonly ILogger logger;

    /// <summary>
    /// Creates new instance
    /// </summary>
    /// <param name="httpClient">Http client</param>
    /// <param name="boxerTokenProvider">Token provider</param>
    /// <param name="logger"></param>
    protected BaseApiClient(HttpClient httpClient, IBoxerTokenProvider boxerTokenProvider, ILogger logger)
    {
        this.httpClient = httpClient;
        this.boxerTokenProvider = boxerTokenProvider;
        this.logger = logger;
    }

    /// <summary>
    /// Executes Http request with Boxer authentication and renews token if needed
    /// </summary>
    /// <param name="request">Prepared HTTP request</param>
    /// <param name="getExternalTokenAsync">Returns external Authorization provider access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Http response</returns>
    protected async Task<HttpResponseMessage> SendBoxerAuthenticatedRequestAsync(HttpRequestMessage request,
        Func<Task<string>> getExternalTokenAsync,
        CancellationToken cancellationToken)
    {
        var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: async (result, _) =>
                {
                    if (result.Result.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        this.logger.LogInformation("Refreshing access token");
                        var token = await this.boxerTokenProvider.GetTokenAsync(refresh: true, getExternalTokenAsync, cancellationToken);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                    else
                    {
                        logger.LogError(result.Exception, "Request error: {ResultStatusCode}, {ResultReasonPhrase}", 
                            result.Result.StatusCode, result.Result.ReasonPhrase);
                    }
                }
            )
            .ExecuteAsync(async () => await httpClient.SendAsync(CloneRequest(request), cancellationToken));
        return response;
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var newRequest = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
        };
        foreach (var header in request.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return newRequest;
    }
}