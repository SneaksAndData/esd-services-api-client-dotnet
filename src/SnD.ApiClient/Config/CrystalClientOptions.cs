namespace SnD.ApiClient.Config;

public class CrystalClientOptions
{
    /// <summary>
    /// Base URI of the Crystal instance
    /// </summary>
    public string BaseUri { get; set; }
    
    
    /// <summary>
    /// Api version
    /// </summary>
    public string ApiVersion { get; set; }
    
    /// <summary>
    /// Maximum retries before giving up on 404 responses.
    /// </summary>
    public int MaxRetries { get; set; }
}
