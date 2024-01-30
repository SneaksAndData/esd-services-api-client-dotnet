using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Base;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Boxer.Exceptions;
using SnD.ApiClient.Boxer.Models;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Boxer;

public class BoxerClient : SndApiClient, IBoxerClient
{
    private readonly Uri baseUri;

    public BoxerClient
    (IOptions<BoxerClientOptions> boxerClientOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<BoxerClient> logger) : base(httpClient, boxerConnector,
        logger)
    {
        baseUri = new Uri(boxerClientOptions.Value.BaseUri
                          ?? throw new ArgumentNullException(nameof(CrystalClientOptions.BaseUri)));
    }

    public async Task<bool> CreateUserAsync(string userId, string provider, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"claim/{provider}/{userId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        return response.EnsureSuccessStatusCode().IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(string userId, string provider, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"claim/{provider}/{userId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => throw new UserNotFoundException(userId, provider),
            _ => response.EnsureSuccessStatusCode().IsSuccessStatusCode
        };
    }

    public async Task<IEnumerable<BoxerJwtClaim>> GetClaimsByUserIdAsync(string userId, string provider,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"claim/{provider}/{userId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => throw new UserNotFoundException(userId, provider),
            _ => BoxerJwtClaim.FromBoxerClaimsApiResponse(await response.EnsureSuccessStatusCode().Content
                .ReadAsStringAsync())
        };
    }

    public async Task<bool> PatchClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"claim/{provider}/{userId}", UriKind.Relative));
        var requestBody = BoxerClaimsApiPatchBody.CreateInsertOperation(claims);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => throw new UserNotFoundException(userId, provider),
            _ => response.EnsureSuccessStatusCode().IsSuccessStatusCode
        };
    }

    public async Task<bool> DeleteClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"claim/{provider}/{userId}", UriKind.Relative));
        var requestBody = BoxerClaimsApiPatchBody.CreateDeleteOperation(claims);
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => throw new UserNotFoundException(userId, provider),
            _ => response.EnsureSuccessStatusCode().IsSuccessStatusCode
        };
    }
}
