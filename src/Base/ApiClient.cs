using System.Net;
using System.Net.Http.Headers;
using ESD.ApiClient.Boxer;
using Polly;

namespace ESD.ApiClient.Base;

public abstract class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly IBoxerTokenProvider _boxerTokenProvider;

    protected ApiClient(HttpClient httpClient, IBoxerTokenProvider boxerTokenProvider)
    {
        this.httpClient = httpClient;
        this._boxerTokenProvider = boxerTokenProvider;
    }

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
                        var token = await this._boxerTokenProvider.GetTokenAsync(refresh: true, getExternalTokenAsync, cancellationToken);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
            )
            .ExecuteAsync(async () => await httpClient.SendAsync(request, cancellationToken));
        return response;
    }
}