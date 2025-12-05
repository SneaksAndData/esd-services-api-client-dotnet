using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly;
using SnD.ApiClient.Boxer.Base;

namespace SnD.ApiClient.Base;

/// <summary>
/// Base class for all API clients
/// </summary>
public abstract class SndApiClient
{
    protected static JsonSerializerOptions JsonSerializerOptions => new() { PropertyNameCaseInsensitive = true };
    
    private readonly HttpClient httpClient;
    private readonly IJwtTokenExchangeProvider boxerConnector;
    protected readonly ILogger logger;
    private string boxerToken;

    protected SndApiClient(HttpClient httpClient, IJwtTokenExchangeProvider boxerConnector, ILogger logger)
    {
        this.httpClient = httpClient;
        this.boxerConnector = boxerConnector;
        this.boxerToken = null;
        this.logger = logger;
    }

    /// <summary>
    /// Executes Http request with Boxer authentication and renews token if needed
    /// </summary>
    /// <param name="request">Prepared HTTP request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Http response</returns>
    protected async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: (result, _) =>
                {
                    logger.LogError(result.Exception, "Request error: {ResultStatusCode}, {ResultReasonPhrase}", 
                        result.Result.StatusCode, result.Result.ReasonPhrase);
                    return Task.CompletedTask;
                }
            )
            .ExecuteAsync(async () => await ExecuteHttpRequest(request, cancellationToken));
        return response;
    }
    
    /// <summary>
    /// Executes Http request without Boxer authentication
    /// </summary>
    /// <param name="request">Prepared HTTP request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Http response</returns>
    protected async Task<HttpResponseMessage> SendAnonymousRequestAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: (result, _) =>
                {
                    logger.LogError(result.Exception, "Request error: {ResultStatusCode}, {ResultReasonPhrase}", 
                        result.Result.StatusCode, result.Result.ReasonPhrase);
                    return Task.CompletedTask;
                }
            )
            .ExecuteAsync(async () => await httpClient.SendAsync(request, cancellationToken));
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(this.boxerToken))
        {
            this.logger.LogInformation("No access token cached - fetching a new one");
            this.boxerToken = await this.boxerConnector.GetTokenAsync(refresh: true, cancellationToken);
        }
        
        var requestClone = CloneRequest(request);
        requestClone.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.boxerToken);
        var response = await httpClient.SendAsync(requestClone, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var newRequest = CloneRequest(request);
            this.logger.LogInformation("Refreshing access token");
            this.boxerToken = await this.boxerConnector.GetTokenAsync(refresh: true, cancellationToken);
            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.boxerToken);
            response = await httpClient.SendAsync(newRequest, cancellationToken);
        }
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
