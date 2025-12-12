namespace SnD.ApiClient.Config;

public class NexusClientOptions
{
    /// <summary>
    /// Base URI of the Crystal instance
    /// </summary>
    public string BaseUri { get; set; }
    
    /// <summary>
    /// Maximum number of retry attempts for transient failures
    /// </summary>
    public int MaxRetryAttempts { get; set; }
    
    /// <summary>
    /// Retry interval in seconds between retry attempts
    /// </summary>
    public int RetryIntervalSeconds { get; set; }
}
