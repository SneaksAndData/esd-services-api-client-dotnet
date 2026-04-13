using KiotaPosts.Client.Models.Models;

namespace SnD.ApiClient.Tests.Acceptance.Config;

public class AcceptanceTestsConfiguration
{
    /// <summary>
    /// Algorithm request
    /// </summary>
    public AlgorithmRequest AlgorithmRequest { get; set; }
    
    /// <summary>
    /// Name of the algorithm
    /// </summary>
    public string AlgorithmName { get; set; }
    
    /// <summary>
    /// Payload serialized to string
    /// </summary>
    public string AlgorithmPayload { get; set; }
    
    /// <summary>
    /// Use BoxerTokenProvider as authentication provider for NexusClient on Azure environment.
    /// If false will add EmptyTokenProvider which does not interact with Boxer.
    /// </summary>
    public bool UseBoxerTokenProviderOnAzure { get; set; }
}