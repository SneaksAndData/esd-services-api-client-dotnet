using KiotaPosts.Client.Models.Models;
using SnD.ApiClient.Nexus.Models;

namespace SnD.ApiClient.Nexus;

public static class AlgorithmRequestExtensions
{
    /// <summary>
    /// Converts AlgorithmRequest without payload to NexusAlgorithmRequest with payload
    /// </summary>
    /// <param name="algorithmRequest">Algorithm request without payload</param>
    /// <param name="payload">JSON serialized payload</param>
    /// <returns>NexusAlgorithmRequest with payload that can be used to create a new run</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static NexusAlgorithmRequest ToNexusAlgorithmRequest(this AlgorithmRequest algorithmRequest, string payload)
    {
        if (algorithmRequest == null)
        {
            throw new ArgumentNullException(nameof(algorithmRequest));
        }
        
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }
        
        return NexusAlgorithmRequest.Create(
            payload,
            algorithmRequest.CustomConfiguration,
            algorithmRequest.ParentRequest,
            algorithmRequest.PayloadValidFor,
            algorithmRequest.RequestApiVersion,
            algorithmRequest.Tag
        );
    }
}