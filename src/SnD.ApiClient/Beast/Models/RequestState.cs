using System.Text.Json.Serialization;

namespace SnD.ApiClient.Beast.Models;

/// <summary>
/// Keeps the info of a buffered request
/// </summary>

public record RequestState : RequestBase
{
    /// <summary>
    /// IP/API address of a processing cluster.
    /// </summary>
    public string ProcessingCluster { get; set; }

    /// <summary>
    /// Request lifecycle stage.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BeastRequestLifeCycleStage LifeCycleStage { get; set; }

    /// <summary>
    /// Request identifier in a buffer.
    /// </summary>
    public string MessageId { get; set; }

    /// <summary>
    /// Unique token used to remove request from a buffer.
    /// </summary>
    public string Receipt { get; set; }

    /// <summary>
    /// Request try number.
    /// </summary>
    public int? TryNumber { get; set; }

    [JsonIgnore]
    public bool IsCompleted =>
        this.LifeCycleStage is BeastRequestLifeCycleStage.FAILED or BeastRequestLifeCycleStage.COMPLETED;
}
