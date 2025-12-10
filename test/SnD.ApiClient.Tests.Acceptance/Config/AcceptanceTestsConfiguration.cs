using KiotaPosts.Client.Models.Models;
using SnD.ApiClient.Nexus.Models;

namespace SnD.ApiClient.Tests.Acceptance.Config;

public class AcceptanceTestsConfiguration
{
    /// <summary>
    /// Algorithm request
    /// </summary>
    public NexusAlgorithmRequest AlgorithmRequest { get; set; }
    
    /// <summary>
    /// Name of the algorithm
    /// </summary>
    public string AlgorithmName { get; set; }
}