namespace SnD.ApiClient.Nexus.Models;

using System.Collections.Generic;

/// <summary>
/// Contains metadata and runtime configuration for an algorithm run request.
/// </summary>
public class RequestMetadata
{
    /// <summary>
    /// The name of the algorithm.
    /// </summary>
    public string? Algorithm { get; set; }

    /// <summary>
    /// The unique identifier of the request.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The cause of algorithm failure, if any.
    /// </summary>
    public string? AlgorithmFailureCause { get; set; }

    /// <summary>
    /// Detailed information about the algorithm failure, if any.
    /// </summary>
    public string? AlgorithmFailureDetails { get; set; }

    /// <summary>
    /// The API version used for the request.
    /// </summary>
    public string? ApiVersion { get; set; }

    /// <summary>
    /// The applied configuration for the request.
    /// </summary>
    public Dictionary<string, string>? AppliedConfiguration { get; set; }

    /// <summary>
    /// Configuration overrides applied to the request.
    /// </summary>
    public Dictionary<string, string>? ConfigurationOverrides { get; set; }

    /// <summary>
    /// The content hash of the request payload.
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// The unique identifier of the job.
    /// </summary>
    public string? JobUid { get; set; }

    /// <summary>
    /// The last modified timestamp of the request.
    /// </summary>
    public string? LastModified { get; set; }

    /// <summary>
    /// The current lifecycle stage of the request.
    /// </summary>
    public string? LifecycleStage { get; set; }

    /// <summary>
    /// Information about the parent job, if any.
    /// </summary>
    public Dictionary<string, string>? ParentJob { get; set; }

    /// <summary>
    /// The URI of the request payload.
    /// </summary>
    public string? PayloadUri { get; set; }

    /// <summary>
    /// The validity period for the payload.
    /// </summary>
    public string? PayloadValidFor { get; set; }

    /// <summary>
    /// The timestamp when the request was received.
    /// </summary>
    public string? ReceivedAt { get; set; }

    /// <summary>
    /// The host that received the request.
    /// </summary>
    public string? ReceivedByHost { get; set; }

    /// <summary>
    /// The URI of the result.
    /// </summary>
    public string? ResultUri { get; set; }

    /// <summary>
    /// The timestamp when the request was sent.
    /// </summary>
    public string? SentAt { get; set; }

    /// <summary>
    /// The client-side assigned tag for the request.
    /// </summary>
    public string? Tag { get; set; }
}