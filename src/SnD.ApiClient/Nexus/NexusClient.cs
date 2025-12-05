using System.Text.Json;
using KiotaPosts.Client;
using KiotaPosts.Client.Algorithm.V1.Run.Item;
using KiotaPosts.Client.Models.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using Polly;
using Polly.Retry;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;
using SnD.ApiClient.Nexus.Models.Extensions;

namespace SnD.ApiClient.Nexus;

public class NexusClient : INexusClient
{
    private readonly NexusGeneratedClient client;
    private readonly AsyncRetryPolicy<WithAlgorithmNamePostResponse?> retryPolicy;
    private readonly ILogger<NexusClient> logger;

    public NexusClient(IRequestAdapter adapter, AsyncRetryPolicy<WithAlgorithmNamePostResponse?> retryPolicy,
        ILogger<NexusClient> logger)
    {
        this.client = new NexusGeneratedClient(adapter);
        this.retryPolicy = retryPolicy;
        this.logger = logger;
    }

    public async Task<CreateRunResponse> CreateRunAsync(JsonElement algorithmParameters,
        string algorithm,
        CustomRunConfiguration? customConfiguration,
        ParentRequest? parentRequest,
        string? tag,
        string? payloadValidFor,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var request = new global::KiotaPosts.Client.Models.Models.AlgorithmRequest
        {
            // AlgorithmParameters = algorithmParameters,
            // CustomConfiguration = customConfiguration.ToNexusAlgorithmSpec(),
            // ParentRequest = parentRequest.ToAlgorithmRequestRef(),
            // Tag = tag,
            // PayloadValidFor = payloadValidFor
        };

        var response = await this.retryPolicy.ExecuteAsync(async () =>
        {
            return await this.client.Algorithm.V1.Run[algorithm]
                .PostAsWithAlgorithmNamePostResponseAsync(request,
                    config => { config.QueryParameters.DryRun = dryRun.ToString(); }, cancellationToken);
        });

        return new CreateRunResponse
        {
            RequestId = response?.AdditionalData["requestId"].ToString()
        };
    }

    public async Task<RunResult> AwaitRunAsync(string requestId, string algorithm, TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await client.Algorithm.V1.Results[algorithm].Requests[requestId]
                .GetAsync(requestConfiguration: null, cancellationToken);
            if (result != null && IsFinished(result))
            {
                return new RunResult(); // TODO
            }

            logger.LogInformation(
                "Run {RequestId} for algorithm {Algorithm} not finished yet. Waiting {PollInterval} before next check.",
                requestId, algorithm, pollInterval);
            await Task.Delay(pollInterval, cancellationToken);
        }
    }

    public async Task<List<RunResult>> AwaitTaggedRunsAsync(ICollection<string> tags,
        string? algorithm,
        TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        var ids = await this.GetRunsByTags(tags, algorithm, cancellationToken);

        var results = new List<RunResult>();
        foreach (var (expectedAlgorithm, requestId) in ids)
        {
            var result = await this.AwaitRunAsync(requestId, expectedAlgorithm, pollInterval, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<CheckpointedRequest?> GetRequestMetadataAsync(string requestId, string algorithm, CancellationToken cancellationToken = default)
    {
        return await this.client.Algorithm.V1.Metadata[algorithm].Requests[requestId].GetAsync(null, cancellationToken);
    }

    public async Task CancelRunAsync(string requestId, string algorithm, string initiator, string reason,
        string policy = "Background", CancellationToken cancellationToken = default)
    {
        var request = new CancellationRequest
        {
            Initiator = initiator,
            Reason = reason,
            CancellationPolicy = policy,
        };
        await this.client.Algorithm.V1.Cancel[algorithm].Requests[requestId]
            .PostAsync(request, null, cancellationToken);
    }

    public bool IsFinished(RequestResult result)
    {
        throw new NotImplementedException();
    }

    public bool IsFinished(RunResult result)
    {
        throw new NotImplementedException();
    }

    public bool? HasSucceeded(RunResult result)
    {
        throw new NotImplementedException();
    }

    private async Task<List<(string, string)>> GetRunsByTags(ICollection<string> tags, string? algorithmName,
        CancellationToken cancellationToken)
    {
        var ids = new List<(string, string)>();
        foreach (var tag in tags)
        {
            var results = await this.client.Algorithm.V1.Results.Tags[tag].GetAsync(null, cancellationToken);
            if (results == null)
            {
                logger.LogWarning("No results found for tag {Tag}, skipping.", tag);
                continue;
            }

            foreach (var result in results)
            {
                if (result.RequestId == null)
                {
                    logger.LogWarning("Tagged result with tag {Tag} has no RequestId, skipping.", tag);
                    continue;
                }

                if (result.AlgorithmName == null)
                {
                    logger.LogWarning("Tagged result with tag {Tag} has no AlgorithmName, skipping.", tag);
                    continue;
                }

                if (algorithmName == null || result.AlgorithmName == algorithmName)
                {
                    ids.Add((result.AlgorithmName, result.RequestId));
                }
            }
        }

        return ids;
    }
}