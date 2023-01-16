using ESD.ApiClient.Crystal.Models.Base;

namespace ESD.ApiClient.Tests.Acceptance.Config;

public class AcceptanceTestsConfiguration
{
    /// <summary>
    /// Algorithm configuration
    /// </summary>
    public AlgorithmConfiguration AlgorithmConfiguration { get; set; }
    
    /// <summary>
    /// Payload serialized to string
    /// </summary>
    public string AlgorithmPayload { get; set; }
    
    /// <summary>
    /// Name of the algorithm
    /// </summary>
    public string AlgorithmName { get; set; }
}