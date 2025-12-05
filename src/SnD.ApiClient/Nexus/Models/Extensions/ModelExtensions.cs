using KiotaPosts.Client.Models.Models;
using KiotaPosts.Client.Models.V1;

namespace SnD.ApiClient.Nexus.Models.Extensions;

public static class ModelExtensions
{
    public static AlgorithmRequestRef? ToAlgorithmRequestRef(this ParentRequest? parentRequest)
    {
        if (parentRequest == null)
        {
            return null;
        }
        return new AlgorithmRequestRef
         {
            AlgorithmName = parentRequest.AlgorithmName,
            RequestId = parentRequest.RequestId
         };
   }

    // public static NexusAlgorithmSpec? ToNexusAlgorithmSpec(this CustomRunConfiguration? customRunConfiguration)
    // {
    //     
    //     var spec = new NexusAlgorithmSpec
    //     {
    //         Args = "",
    //         Command = "",
    //         ComputeResources = null,
    //         Container = null,
    //         DatadogIntegrationSettings           = null,
    //         ErrorHandlingBehaviour = null,
    //     }
    //     
    // }
        
}