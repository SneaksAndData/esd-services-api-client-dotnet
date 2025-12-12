using KiotaPosts.Client;
using KiotaPosts.Client.Models.Models;
using KiotaPosts.Client.Models.V1;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;

namespace SnD.ApiClient.Nexus;

public class NexusClient(IRequestAdapter adapter, ILogger<NexusClient> logger) : INexusClient
{
    private readonly NexusGeneratedClient client = new(adapter);

    public async Task<CreateRunResponse> CreateRunAsync(NexusAlgorithmRequest algorithmRequest,
        string algorithm,
        NexusAlgorithmSpec? customConfiguration,
        AlgorithmRequestRef? parentRequest,
        string? tag,
        string? payloadValidFor,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var response = await this.client.Algorithm.V1.Run[algorithm]
            .PostAsWithAlgorithmNamePostResponseAsync(algorithmRequest,
                config =>
                {
                    config.QueryParameters.DryRun = dryRun.ToString();
                }, cancellationToken);

        return new CreateRunResponse
        {
            RequestId = response?.AdditionalData["requestId"].ToString()
        };
    }

    public async Task<RequestResult> AwaitRunAsync(string requestId, string algorithm, TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await client.Algorithm.V1.Results[algorithm].Requests[requestId]
                .GetAsync(requestConfiguration: null, cancellationToken);
            if (result != null && IsFinished(result))
            {
                return result;
            }

            logger.LogInformation(
                "Run {RequestId} for algorithm {Algorithm} not finished yet. Waiting {PollInterval} before next check.",
                requestId, algorithm, pollInterval);
            await Task.Delay(pollInterval, cancellationToken);
        }
    }

    public async Task<List<RequestResult>> AwaitTaggedRunsAsync(ICollection<string> tags,
        string? algorithm,
        TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        var ids = await this.GetRunsByTags(tags, algorithm, cancellationToken);

        var results = new List<RequestResult>();
        foreach (var (expectedAlgorithm, requestId) in ids)
        {
            var result = await this.AwaitRunAsync(requestId, expectedAlgorithm, pollInterval, cancellationToken);
            results.Add(result);
        }

        return results;
    }

    public async Task<CheckpointedRequest?> GetRequestMetadataAsync(string requestId, string algorithm,
        CancellationToken cancellationToken = default)
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
        await this.client.Algorithm.V1.Cancel[algorithm].Requests[requestId].PostAsync(request, null, cancellationToken);
    }

    public bool IsFinished(RequestResult result)
    {
        return result.Status?.ToLowerInvariant() switch
        {
            "failed" => true,
            "completed" => true,
            "deadline_exceeded" => true,
            "scheduling_failed" => true,
            "cancelled" => true,
            _ => false
        };
    }

    public bool? HasSucceeded(RequestResult result)
    {
        if (!IsFinished(result))
        {
            return null;
        }
        return result.Status?.ToLowerInvariant() == "completed";
    }

    private async Task<IEnumerable<(string, string)>> GetRunsByTags(ICollection<string> tags, string? algorithmName,
        CancellationToken cancellationToken)
    {

        var tasks = tags
            .Select(async tag => (await this.client.Algorithm.V1.Results.Tags[tag].GetAsync(null, cancellationToken), tag))
            .ToArray()
            .Select(async taskWithTag =>
            {
                var (results, tag) = await taskWithTag;
                if (results == null)
                {
                    logger.LogWarning("No results found for tag {Tag}, skipping", tag);
                    return [];
                }
                return ExtractRequestId(results, tag, algorithmName);
            });

        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r);
    }

    private IEnumerable<(string, string)> ExtractRequestId(IEnumerable<TaggedRequestResult> results, string tag,
        string? algorithmName)
    {
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
                yield return (result.AlgorithmName, result.RequestId);
            }
        }
    }
}