using System.Text.Json;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;

namespace ESD.ApiClient.Crystal;

public interface ICrystalConnector
{
    /// <summary>
    /// Creates new run for of specified algorithm in Crystal
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="payload">Algorithm payload</param>
    /// <param name="customConfiguration">Custom configuration for algorithm run</param>
    /// <param name="getTokenAsync">Function that return JWT for authentication</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Instance of object <see cref="CreateRunResponse"/> with run ID</returns>
    Task<CreateRunResponse?> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration, Func<Task<string>> getTokenAsync,
        CancellationToken cancellationToken);
}