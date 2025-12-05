using System.Text.Json;
using KiotaPosts.Client;
using Microsoft.Kiota.Abstractions;
using SnD.ApiClient.Nexus.Base;
using SnD.ApiClient.Nexus.Models;

namespace SnD.ApiClient.Nexus;

public class NexusClient: INexusClient
{
    private readonly NexusGeneratedClient client;
    
    public NexusClient(IRequestAdapter adapter)
    {
        this.client = new NexusGeneratedClient(adapter); 
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
            // CustomConfiguration = customConfiguration,
            // ParentRequest = parentRequest,
            // Tag = tag,
            // PayloadValidFor = payloadValidFor
        };

        var response = await this.client.Algorithm.V1.Run[algorithm]
            .PostAsWithAlgorithmNamePostResponseAsync(request, config =>
            {
                config.QueryParameters.DryRun = dryRun.ToString();
            }, cancellationToken);
         
         return new CreateRunResponse
         {
             RequestId = response?.AdditionalData["requestId"].ToString()
         };
    }

    public Task<RunResult> AwaitRunAsync(string requestId, string algorithm, TimeSpan pollInterval, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<RunResult> AwaitTaggedRunsAsync(ICollection<string> tags, string? algorithm, TimeSpan pollInterval,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public RequestMetadata? GetRequestMetadata(string requestId, string algorithm)
    {
        throw new NotImplementedException();
    }

    public void CancelRun(string requestId, string algorithm, string initiator, string reason, string policy = "Background")
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
}