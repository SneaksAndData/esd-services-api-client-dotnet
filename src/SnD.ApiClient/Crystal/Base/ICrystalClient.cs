using System.Text.Json;
using SnD.ApiClient.Base.Models;
using SnD.ApiClient.Crystal.Models;
using SnD.ApiClient.Crystal.Models.Base;
using SnD.ApiClient.Exceptions;

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
    /// Creates new run for of specified algorithm in Crystal using tagging and concurrency strategy.
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="payload">Algorithm payload</param>
    /// <param name="customConfiguration">Custom configuration for algorithm run</param>
    /// <param name="tagId">Tag for concurrency tracking</param>
    /// <param name="concurrencyStrategy">Concurrency strategy</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Instance of object <see cref="CreateRunResponse"/> with run ID</returns>
    /// <exception cref="ConcurrencyError">If there is already a run with the same tag and ConcurrencyStrategy is set to <see cref="ConcurrencyStrategy.SKIP"/></exception>
    Task<CreateRunResponse> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration,
        string tagId, ConcurrencyStrategy concurrencyStrategy,
        CancellationToken cancellationToken);

    /// <summary>
    /// Query and return result of the run
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="requestId">Request ID received form <see cref="CreateRunAsync"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>RunResult instance</returns>
    public Task<RunResult> GetResultAsync(string algorithm, string requestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Awaits a run until it completes with any result or runs out of time set via cancellationToken.
    /// For some cases this can return a failed result when the algorithm has not been actually executed, for example,
    /// when a submission was lost.
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="requestId">Request ID received form <see cref="CreateRunAsync"/></param>
    /// <param name="cancellationToken">Cancellation token for the operation timeout.</param>
    /// <param name="pollInterval">Poll interval to check for run results.</param>
    /// <returns>RunResult instance</returns>
    public Task<RunResult> AwaitRunAsync(string algorithm, string requestId, TimeSpan pollInterval,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the result of the run and converts it to the specified type using a specified converter function.
    /// If the run is not completed yet or has failed, the method will return default value for the type.
    /// </summary>
    /// <param name="algorithm">Algorithm name</param>
    /// <param name="requestId">Request ID received form <see cref="CreateRunAsync"/></param>
    /// <param name="cancellationToken">Cancellation token for the operation timeout.</param>
    /// <param name="converter">Function to convert bytes from results into TResult</param>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <returns></returns>
    public Task<TResult> GetResultAsync<TResult>(string algorithm, string requestId, Func<byte[], TResult> converter,
        CancellationToken cancellationToken = default);
}
