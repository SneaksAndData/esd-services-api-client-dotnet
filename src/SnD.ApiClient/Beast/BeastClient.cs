using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Base;
using SnD.ApiClient.Base.Models;
using SnD.ApiClient.Beast.Base;
using SnD.ApiClient.Beast.Models;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Exceptions;

namespace SnD.ApiClient.Beast;

public class BeastClient : SndApiClient, IBeastClient
{
    private readonly Uri baseUri;

    private readonly HashSet<BeastRequestLifeCycleStage> completedStages = new()
    {
        BeastRequestLifeCycleStage.COMPLETED,
        BeastRequestLifeCycleStage.FAILED,
        BeastRequestLifeCycleStage.STALE,
        BeastRequestLifeCycleStage.SCHEDULING_FAILED,
        BeastRequestLifeCycleStage.SUBMISSION_FAILED
    };

    public BeastClient(IOptions<BeastClientOptions> beastClientOptions, HttpClient httpClient,
        IJwtTokenExchangeProvider boxerConnector, ILogger<BeastClient> logger) : base(httpClient, boxerConnector,
        logger)
    {
        baseUri = new Uri(beastClientOptions.Value.BaseUri
                          ?? throw new ArgumentNullException(nameof(BeastClientOptions.BaseUri)));
    }

    /// <inheritdoc />
    public async Task<RequestState> SubmitJobAsync(JobRequest jobParams, string submissionConfigurationName,
        CancellationToken cancellationToken = default, ConcurrencyStrategy? concurrencyStrategy= null) 
    {
        cancellationToken.ThrowIfCancellationRequested();

        concurrencyStrategy ??= ConcurrencyStrategy.IGNORE;
        if (concurrencyStrategy != ConcurrencyStrategy.IGNORE)
        {
            if (string.IsNullOrEmpty(jobParams.ClientTag))
            {
                throw new ArgumentException("You must supply a client tag when using a concurrency strategy");
            }

            var existingJobIds = await GetJobIdsByTagAsync(jobParams.ClientTag, cancellationToken);

            var incompleteJobs = (await Task.WhenAll(existingJobIds
                    .Select(async id => await GetJobStateAsync(id, cancellationToken))))
                .Where(state => !completedStages.Contains(state.LifeCycleStage)).ToArray();

            if (incompleteJobs.Any())
            {
                switch (concurrencyStrategy)
                {
                    case ConcurrencyStrategy.SKIP:
                        throw new ConcurrencyError(concurrencyStrategy.Value, incompleteJobs.First().Id,
                            jobParams.ClientTag);
                    case ConcurrencyStrategy.AWAIT:
                        await Task.WhenAll(incompleteJobs
                            .Select(job => AwaitRunAsync(job.Id, TimeSpan.FromSeconds(5), cancellationToken)));
                        break;
                    case ConcurrencyStrategy.REPLACE:
                        throw new NotImplementedException("ConcurrencyStrategy.REPLACE not implemented for BEAST");
                }
            }
        }
        if(Regex.IsMatch(jobParams.ClientTag ?? "", @"[^\w\d\-\._~]"))
        {
            throw new ArgumentException("ClientTag can only contain alphanumeric characters, hyphens, periods, underscores, and tildes");
        }
        
        var requestUri = new Uri(baseUri, new Uri($"job/submit/{submissionConfigurationName}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(jobParams), Encoding.UTF8, "application/json")
        };
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RequestState>(
            await response.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }

    /// <inheritdoc />
    public async Task<RequestState> AwaitRunAsync(string requestId, TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        RequestState result = null;

        if (cancellationToken == CancellationToken.None)
        {
            throw new ArgumentException("Cancellation token None is not allowed.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        do
        {
            await Task.Delay(pollInterval, cancellationToken);
            result = await GetRequestState(requestId, cancellationToken);
            if (completedStages.Contains(result.LifeCycleStage))
            {
                return result;
            }
        } while (!cancellationToken.IsCancellationRequested);

        return result;
    }

    /// <inheritdoc />
    public Task<RequestState> GetJobStateAsync(string requestId, CancellationToken cancellationToken = default)
    {
        return GetRequestState(requestId, cancellationToken);
    }


    private async Task<string[]> GetJobIdsByTagAsync(string clientTag, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"job/requests/tags/{clientTag}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<string[]>(await response.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }

    private async Task<RequestState> GetRequestState(string requestId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri($"job/requests/{requestId}", UriKind.Relative));
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await SendAuthenticatedRequestAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<RequestState>(
            await response.Content.ReadAsStringAsync(cancellationToken),
            JsonSerializerOptions);
    }
}
