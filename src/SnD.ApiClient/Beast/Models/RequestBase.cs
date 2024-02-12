namespace SnD.ApiClient.Beast.Models;

public abstract record RequestBase
{
    /// <summary>
    /// Unique request identifier assigned after successful buffering.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Request client tag.
    /// </summary>
    public string ClientTag { get; set; }
    
    /// <summary>
    /// Request last modified timestamp.
    /// </summary>
    public DateTimeOffset? LastModified { get; set; }
}
