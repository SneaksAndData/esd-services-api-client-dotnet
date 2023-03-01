using System.Text.Json.Serialization;

namespace SnD.ApiClient.Crystal.Models.Base;

public class RunResult
{
    /// <summary>
    /// Identifier of a request.
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// Processing status of a request.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RequestLifeCycleStage Status { get; set; }

    /// <summary>
    /// URI to download request results.
    /// </summary>
    public Uri ResultUri { get; set; }

    /// <summary>
    /// Error to be communicated to a client.
    /// </summary>
    public string RunErrorMessage { get; set; }
}