using System.Text.Json;
using SnD.ApiClient.Crystal.Models;
using SnD.ApiClient.Crystal.Models.Base;

namespace SnD.ApiClient.Crystal.Base;

public interface ICrystalClient
{
    /// <summary>
    /// Creates new run for of specified algorithm in Crystal
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="payload">Algorithm payload</param>
    /// <param name="customConfiguration">Custom configuration for algorithm run</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Instance of object <see cref="CreateRunResponse"/> with run ID</returns>
    Task<CreateRunResponse> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration, CancellationToken cancellationToken);

    /// <summary>
    /// Query and return result of the run
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="requestId">Request ID received form <see cref="CreateRunAsync"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RunResult instance</returns>
    public Task<RunResult> GetResultAsync(string algorithm, string requestId, CancellationToken cancellationToken = default);
}