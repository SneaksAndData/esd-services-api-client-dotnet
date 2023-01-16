using System.Text.Json;
using System.ComponentModel.DataAnnotations;

namespace SnD.ApiClient.Crystal.Models.Base;

/// <summary>
/// Data to be submitted by a client, when doing an algorithm invocation.
/// </summary>
public class AlgorithmRequest
{
    /// <summary>
    /// Algorithm-specific configuration. Must be a JSON-serializable object.
    /// </summary>
    [Required]
    public JsonElement  AlgorithmParameters { get; set; }
        
    /// <summary>
    /// Optional custom configuration to be applied when running this algorithm.
    /// </summary>
    public AlgorithmConfiguration? CustomConfiguration { get; set; }
        
    /// <summary>
    /// Tag for tracking request status
    /// </summary>
    public string Tag { get; set; }
}