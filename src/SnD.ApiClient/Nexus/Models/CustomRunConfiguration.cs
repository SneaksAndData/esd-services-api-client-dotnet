using KiotaPosts.Client.Models.V1;

namespace SnD.ApiClient.Nexus.Models;
public class CustomRunConfiguration
{
    /// <summary>
    ///  Algorithm version override
    /// </summary>
    public string? Version { get; set; }
    
    /// <summary>
    ///  Workgroup name override
    /// </summary>
    public string? WorkgroupName { get; set; }
    
    /// <summary>
    ///  Workgroup group override
    /// </summary>
    public string? WorkgroupGroup { get; set; }
    
    /// <summary>
    /// Algorithm workgroup kind override
    /// </summary>
    public string? WorkgroupKind { get; set; }
    
    /// <summary>
    /// CPU limit override
    /// </summary>
    public string? CpuLimit { get; set; }
    
    /// <summary>
    /// Memory limit override
    /// </summary>
    public string? MemoryLimit { get; set; }

    public NexusAlgorithmContainer? Container { get; set; }
}