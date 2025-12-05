using System.Text.Json;
using KiotaPosts.Client.Models.Models;
using SnD.ApiClient.Nexus.Models;

namespace SnD.ApiClient.Nexus.Base;

public interface INexusClient
{
    /// <summary>
    /// Creates a new run for a given algorithm.
    /// </summary>
    /// <param name="algorithmParameters">Algorithm parameters as JsonElement</param>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="customConfiguration">Custom configuration for algorithm run</param>
    /// <param name="parentRequest">Optional Parent request reference, if applicable. Specifying a parent request allows
    /// indirect cancellation of the submission - via cancellation of a parent.</param>
    /// <param name="tag">Client side assigned run tag.</param>
    /// <param name="payloadValidFor">Payload pre-signed URL validity period.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="dryRun">Dry run, if set to True, will only buffer a submission but skip job creation.</param>
    /// <returns>Instance of object <see cref="CreateRunResponse"/> with run ID</returns>
    Task<CreateRunResponse> CreateRunAsync(JsonElement algorithmParameters, string algorithm,
        CustomRunConfiguration? customConfiguration,
        ParentRequest? parentRequest,
        string? tag,
        string? payloadValidFor,
        bool dryRun,
        CancellationToken cancellationToken);

    /// <summary>
    /// Awaits result for a given run for a given algorithm.
    /// </summary>
    /// <param name="requestId">Request ID received form <see cref="CreateRunAsync"/></param>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="pollInterval">Poll interval to check for run results.</param>
    /// <param name="cancellationToken">Cancellation token for the operation timeout.</param>
    /// <returns>RunResult instance</returns>
    public Task<RunResult> AwaitRunAsync(string requestId, string algorithm, TimeSpan pollInterval,
        CancellationToken cancellationToken);

    /// <summary>
    /// Awaits result for a given run for a given algorithm.
    /// </summary>
    /// <param name="tags">Tags to use when filtering runs</param>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="pollInterval">Poll interval to check for run results.</param>
    /// <param name="cancellationToken">Cancellation token for the operation timeout.</param>
    /// <returns>RunResult instance</returns>
    public Task<List<RunResult>> AwaitTaggedRunsAsync(ICollection<string> tags, string? algorithm, TimeSpan pollInterval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns metadata and full runtime configuration for the request container.
    /// </summary>
    /// <param name="requestId">The unique identifier of the request.</param>
    /// <param name="algorithm">The name of the algorithm.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>RequestMetadata instance if found; otherwise, null.</returns>
    public Task<CheckpointedRequest?> GetRequestMetadataAsync(string requestId, string algorithm,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Cancel a run for the provided request id and algorithm.
    /// </summary>
    /// <param name="requestId">Run request identifier.</param>
    /// <param name="algorithm">Algorithm name for the provided identifier.</param>
    /// <param name="initiator">Person initiating the cancel.</param>
    /// <param name="reason">Reason for cancellation.</param>
    /// <param name="policy">Cleanup policy. Default is "Background".</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task CancelRunAsync(string requestId, string algorithm, string initiator, string reason, string policy = "Background",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a run has finished.
    /// </summary>
    /// <param name="result">RunResult instance.</param>
    /// <returns>True if the run has finished; otherwise, false.</returns>
    bool IsFinished(RunResult result);

    /// <summary>
    /// Check if a run has succeeded. Returns null if the run is not finished yet.
    /// </summary>
    /// <param name="result">RunResult instance.</param>
    /// <returns>True if succeeded, false if failed, or null if not finished yet.</returns>
    bool? HasSucceeded(RunResult result);
}