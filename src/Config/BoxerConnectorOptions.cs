namespace SnD.ApiClient.Config;

public class BoxerConnectorOptions
{
    /// <summary>
    /// Base URI of the boxer instance
    /// </summary>
    public string BaseUri { get; set; }
    
    /// <summary>
    /// Name of authorization provider
    /// </summary>
    public string IdentityProvider { get; set; }
}