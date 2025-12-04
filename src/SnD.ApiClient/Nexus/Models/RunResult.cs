namespace SnD.ApiClient.Nexus.Models;

/// <summary>
/// C# SDK data structure for RunResult.
/// </summary>
public class RunResult
{
    /// <summary>
    /// The algorithm name.
    /// </summary>
    public string? Algorithm { get; set; }

    /// <summary>
    /// The request identifier.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// The result URI.
    /// </summary>
    public string? ResultUri { get; set; }

    /// <summary>
    /// The run error message.
    /// </summary>
    public string? RunErrorMessage { get; set; }

    /// <summary>
    /// The client error type.
    /// </summary>
    public string? ClientErrorType { get; set; }

    /// <summary>
    /// The client error message.
    /// </summary>
    public string? ClientErrorMessage { get; set; }

    /// <summary>
    /// The run status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Checks if this object is empty (end of the response).
    /// </summary>
    /// <returns>True if all properties are null; otherwise, false.</returns>
    public bool IsEmpty()
    {
        return Algorithm == null
               && RequestId == null
               && ResultUri == null
               && RunErrorMessage == null
               && Status == null
               && ClientErrorType == null
               && ClientErrorMessage == null;
    }
}