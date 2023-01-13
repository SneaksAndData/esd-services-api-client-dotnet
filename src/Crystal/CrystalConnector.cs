using System.Text;
using System.Text.Json;
using ESD.ApiClient.Base;
using ESD.ApiClient.Boxer.Base;
using ESD.ApiClient.Config;
using ESD.ApiClient.Crystal.Base;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESD.ApiClient.Crystal;

public class CrystalConnector : BaseApiClient, ICrystalConnector
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    /// <summary>
    /// Creates new instance
    /// </summary>
    /// <param name="httpClient">Http client</param>
    /// <param name="boxerConnector">Boxer token provider instance</param>
    /// <param name="logger">Logger</param>
    public CrystalConnector(IOptions<CrystalConnectorOptions> crystalConnectionOptions, HttpClient httpClient,
        IBoxerConnector boxerConnector, ILogger<CrystalConnector> logger) : base(httpClient, boxerConnector, logger)
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
        var response = SendBoxerAuthenticatedRequestAsync(request, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        var responseString = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateRunResponse>(responseString, JsonSerializerOptions);
    }

    public async Task<RunResult?> QueryResultAsync(string algorithm, string requestId, CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendBoxerAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
    }
}