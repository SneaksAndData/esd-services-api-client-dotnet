using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Base;
using SnD.ApiClient.Beast.Base;
using SnD.ApiClient.Beast.Models;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Beast;

public class BeastClient : SndApiClient, IBeastClient
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    public BeastClient(IOptions<BeastClientOptions> beastClientOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<BeastClient> logger) : base(httpClient, boxerConnector,
        logger)
    {
        apiVersion = beastClientOptions.Value.ApiVersion ??
                     throw new ArgumentNullException(nameof(BeastClientOptions.ApiVersion));
        baseUri = new Uri(beastClientOptions.Value.BaseUri
                          ?? throw new ArgumentNullException(nameof(BeastClientOptions.BaseUri)));
    }

    public async Task<RequestState> SubmitJobAsync(JobRequest jobParams, string submissionConfigurationName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"{apiVersion}/job/submit/{submissionConfigurationName}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content = new StringContent(JsonSerializer.Serialize(jobParams), Encoding.UTF8, "application/json");
        var response = SendAuthenticatedRequestAsync(request, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RequestState>(
            await response.Result.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }
    
    public async Task<RequestState> GetJobStateAsync(string requestId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"{apiVersion}/job/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = SendAuthenticatedRequestAsync(request, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RequestState>(
            await response.Result.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }
}
