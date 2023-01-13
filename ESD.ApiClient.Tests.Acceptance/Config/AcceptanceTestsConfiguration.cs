using System.Text.Json;
using ESD.ApiClient.Crystal.Models.Base;

namespace ESD.ApiClient.Tests.Acceptance.Config;

public class AcceptanceTestsConfiguration
{
    public string AuthenticatinProvider { get; set; }
    public string BoxerBaseUrl { get; set; }
    
    public string CrystalBaseUrl { get; set; }
    
    public AlgorithmConfiguration AlgorithmConfiguration { get; set; }
    
    public string AlgorithmPayload { get; set; }
    public string AlgorithmName { get; set; }
}