using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SnD.ApiClient.Crystal.Models;
using SnD.ApiClient.Crystal.Models.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Base;
using SnD.ApiClient.Base.Models;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal.Base;
using SnD.ApiClient.Exceptions;

namespace SnD.ApiClient.Crystal;

public class CrystalClient : SndApiClient, ICrystalClient
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    private readonly RequestLifeCycleStage[] completedStages =
    {
        RequestLifeCycleStage.FAILED, RequestLifeCycleStage.COMPLETED, RequestLifeCycleStage.DEADLINE_EXCEEDED,
        RequestLifeCycleStage.SCHEDULING_TIMEOUT
    };

    public CrystalClient(IOptions<CrystalClientOptions> crystalClientOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<CrystalClient> logger) : base(httpClient, boxerConnector,
        logger)
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
        return await RunAsync(algorithm, payload, customConfiguration, cancellationToken);
    }

    private async Task<CreateRunResponse> RunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration,
        CancellationToken cancellationToken)
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
    public async Task<CreateRunResponse> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration, string tagId,
        ConcurrencyStrategy concurrencyStrategy, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(tagId))
        {
            throw new ArgumentException("TagId is required for concurrency strategy");
        }
        if(Regex.IsMatch(tagId, @"[^\w\d\-\._~]"))
        {
            throw new ArgumentException("TagId can only contain alphanumeric characters, hyphens, periods, underscores, and tildes");
        }

        if (concurrencyStrategy != ConcurrencyStrategy.IGNORE)
        {
            var runningJobs = await GetJobIdsByTagAsync(algorithm, tagId, cancellationToken);

            var incompleteJobs = runningJobs.Where(job => !completedStages.Contains(job.Status)).ToArray();
            if (incompleteJobs.Any())
            {
                switch (concurrencyStrategy)
                {
                    case ConcurrencyStrategy.SKIP:
                        throw new ConcurrencyError(concurrencyStrategy, incompleteJobs.First().RequestId, tagId);
                    case ConcurrencyStrategy.AWAIT:
                        await Task.WhenAll(incompleteJobs
                            .Select(job =>
                                AwaitRunAsync(algorithm, job.RequestId, TimeSpan.FromSeconds(5), cancellationToken)));
                        break;
                    case ConcurrencyStrategy.REPLACE:
                        throw new NotImplementedException("ConcurrencyStrategy.REPLACE not implemented for Crystal");
                }
            }
        }

        return await RunAsync(algorithm, payload, customConfiguration, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<RunResult> GetResultAsync(string algorithm, string requestId,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(baseUri,
            new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public async Task<RunResult> AwaitRunAsync(string algorithm, string requestId, TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri(baseUri,
            new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        RunResult result = null;

        if (cancellationToken == CancellationToken.None)
        {
            throw new ArgumentException("Cancellation token None is not allowed.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        do
        {
            // Crystal can return 404 in three cases:
            // - Submission is not found
            // - Submission is delayed
            // - Submission was lost
            // For the two latter ones we can wait a bit and see if the situation resolves.
            await Task.Delay(pollInterval, cancellationToken);
            var response = await SendAuthenticatedRequestAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            result = JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(),
                JsonSerializerOptions);

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

    /// <inheritdoc/>
    public async Task<TResult> GetResultAsync<TResult>(string algorithm, string requestId,
        Func<byte[], TResult> converter,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new Uri(baseUri,
            new Uri($"algorithm/{this.apiVersion}/results/{algorithm}/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var runResult =
            JsonSerializer.Deserialize<RunResult>(await response.Content.ReadAsStreamAsync(), JsonSerializerOptions);

        if (runResult.ResultUri == null)
        {
            return default;
        }

        var resultsRequest = new HttpRequestMessage(HttpMethod.Get, runResult.ResultUri);
        var resultData = await SendAnonymousRequestAsync(resultsRequest, cancellationToken);
        resultData.EnsureSuccessStatusCode();

        return converter(await resultData.Content.ReadAsByteArrayAsync());
    }


    private async Task<RunResult[]> GetJobIdsByTagAsync(string algorithm, string clientTag,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri,
            new Uri($"algorithm/{apiVersion}/results/{algorithm}/tags/{clientTag}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = SendAuthenticatedRequestAsync(request, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RunResult[]>(
            await response.Result.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }
}
