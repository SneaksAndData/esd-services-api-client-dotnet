namespace SnD.ApiClient.Beast.Models;

/// <summary>
/// Job inputs for a Beast job
/// </summary>
public record JobRequest
{
    /// <summary>
    /// Input definitions - where to read data from and in what format
    /// </summary>
    public JobDataSocket[] Inputs { get; set; }

    /// <summary>
    /// Output definitions - where to write data to and in what format
    /// </summary>
    public JobDataSocket[] Outputs { get; set; }

    /// <summary>
    /// Any extra args and their values defined by a job's developer
    /// </summary>
    public Dictionary<string, string> ExtraArgs { get; set; }
        
    /// <summary>
    /// Expected number of parallel running tasks in each Spark stage.
    /// </summary>
    public int? ExpectedParallelism { get; set; }

    /// <summary>
    /// Tags to apply when submitting a job, so a client can identify a request w/o knowing the id assigned by Beast
    /// </summary>
    public string ClientTag { get; set; }
}
