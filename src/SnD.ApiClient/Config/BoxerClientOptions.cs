namespace SnD.ApiClient.Config;

public class BoxerClientOptions
{
    /// <summary>
    /// Base URI of the Boxer instance
    /// </summary>
    public string BaseUri { get; set; }
    
    /// <summary>
    /// Uri of the Boxer claims endpoint
    /// </summary>
    public string ClaimsUri { get; set; }
}
