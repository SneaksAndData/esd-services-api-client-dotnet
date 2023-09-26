using System.Text.Json.Serialization;

namespace SnD.ApiClient.Crystal.Models.Base;

public record RunResult
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

    public static RunResult LostSubmission(string requestId)
    {
        return new RunResult
        {
            RequestId = requestId,
            Status = RequestLifeCycleStage.FAILED,
            ResultUri = null,
            RunErrorMessage = "Submission has been lost or does not exist. Please retry it in a few seconds."
        };
    }
    public static RunResult TimeoutSubmission(string requestId)
    {
        return new RunResult
        {
            RequestId = requestId,
            Status = RequestLifeCycleStage.CLIENT_TIMEOUT,
            ResultUri = null,
            RunErrorMessage = "Submission timed out on the client side, but is still running on the server. Note that it might complete eventually, but the client has chosen to give up due to cancellation policy provided."
        };
    }    
}
