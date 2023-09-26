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

public class CrystalClient : SndApiClient, ICrystalClient
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    private readonly RequestLifeCycleStage[] completedStages = {
        RequestLifeCycleStage.FAILED, RequestLifeCycleStage.COMPLETED, RequestLifeCycleStage.DEADLINE_EXCEEDED,
        RequestLifeCycleStage.SCHEDULING_TIMEOUT
    };

    public CrystalClient(IOptions<CrystalClientOptions> crystalClientOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<CrystalClient> logger) : base(httpClient, boxerConnector, logger)
    {
        this.apiVersion = crystalClientOptions.Value.ApiVersion ??
                          throw new ArgumentNullException(nameof(CrystalClientOptions.ApiVersion));
        this.baseUri = new Uri(crystalClientOptions.Value.BaseUri
                       ?? throw new ArgumentNullException(nameof(CrystalClientOptions.BaseUri)));
    }

    /// <inheritdoc/>
    public async Task<CreateRunResponse> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/run/{algorithm}", UriKind.Relative));
        var algorithmRequest = new AlgorithmRequest
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

    /// <inheritdoc/>
    public async Task<RunResult> GetResultAsync(string algorithm, string requestId, CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public async Task<RunResult> AwaitRunAsync(string algorithm, string requestId, CancellationToken cancellationToken = default, int pollIntervalSeconds = 1)
    {
        var requestUri = new Uri(baseUri, new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        RunResult result = null;
        
        cancellationToken.ThrowIfCancellationRequested();
        
        do
        {
            // Crystal can return 404 in three cases:
            // - Submission is not found
            // - Submission is delayed
            // - Submission was lost
            // For the two latter ones we can wait a bit and see if the situation resolves.
            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds), cancellationToken);
            var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            result = JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
            
            if (this.completedStages.Contains(result.Status))
            {
                return result;
            }
            
        } while (!cancellationToken.IsCancellationRequested);

        if (result != null && !this.completedStages.Contains(result.Status))
        {
            result = RunResult.TimeoutSubmission(requestId);
        }

        return result ?? RunResult.LostSubmission(requestId);
    }
}
