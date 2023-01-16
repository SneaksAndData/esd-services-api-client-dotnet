using System.Text;
using System.Text.Json;
using SnD.ApiClient.Crystal.Models;
using SnD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Base;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal.Base;

namespace SnD.ApiClient.Crystal;

public class CrystalConnector : SndApiClient, ICrystalConnector
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    public CrystalConnector(IOptions<CrystalConnectorOptions> crystalConnectionOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<CrystalConnector> logger) : base(httpClient, boxerConnector, logger)
    {
        this.apiVersion = crystalConnectionOptions.Value.ApiVersion ??
                          throw new ArgumentNullException(nameof(CrystalConnectorOptions.ApiVersion));
        this.baseUri = new Uri(crystalConnectionOptions.Value.BaseUri
                       ?? throw new ArgumentNullException(nameof(CrystalConnectorOptions.BaseUri)));
    }

    /// <inheritdoc/>
    public async Task<CreateRunResponse?> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/run/{algorithm}", UriKind.Relative));
        var algorithmRequest = new AlgorithmRequest()
        {
            AlgorithmParameters = payload,
            CustomConfiguration = customConfiguration
        };
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content =
            new StringContent(JsonSerializer.Serialize(algorithmRequest), Encoding.UTF8, "application/json");
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateRunResponse>(responseString, JsonSerializerOptions);
    }

    public async Task<RunResult?> GetResultAsync(string algorithm, string requestId, CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
    }
}