using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ESD.ApiClient.Boxer.Base;
using Microsoft.Extensions.Logging;
using Polly;

namespace SnD.ApiClient.Base;

/// <summary>
/// Base class for all API clients
/// </summary>
public abstract class BaseApiClient
{
    protected static JsonSerializerOptions JsonSerializerOptions => new() { PropertyNameCaseInsensitive = true };
    
    private readonly HttpClient httpClient;
    private readonly IBoxerConnector _boxerConnector;
    private readonly ILogger logger;

    protected BaseApiClient(HttpClient httpClient, IBoxerConnector boxerConnector, ILogger logger)
    {
        this.httpClient = httpClient;
        this._boxerConnector = boxerConnector;
        this.logger = logger;
    }

    /// <summary>
    /// Executes Http request with Boxer authentication and renews token if needed
    /// </summary>
    /// <param name="request">Prepared HTTP request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Http response</returns>
    protected async Task<HttpResponseMessage> SendBoxerAuthenticatedRequestAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetryAsync: async (result, _) =>
                {
                    logger.LogError(result.Exception, "Request error: {ResultStatusCode}, {ResultReasonPhrase}", 
                        result.Result.StatusCode, result.Result.ReasonPhrase);
                }
            )
            .ExecuteAsync(async () => await ExecuteHttpRequest(request, cancellationToken));
        return response;
    }

    private async Task<HttpResponseMessage> ExecuteHttpRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var responce = await httpClient.SendAsync(CloneRequest(request), cancellationToken);
        if (responce.StatusCode == HttpStatusCode.Unauthorized)
        {
            var newRequest = CloneRequest(request);
            this.logger.LogInformation("Refreshing access token");
            var token = await this._boxerConnector.GetTokenAsync(refresh: true, cancellationToken);
            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            responce = await httpClient.SendAsync(newRequest, cancellationToken);
        }

        return responce;
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