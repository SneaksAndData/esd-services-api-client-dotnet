using SnD.ApiClient.Base.Models;
using SnD.ApiClient.Beast.Models;
using SnD.ApiClient.Exceptions;

namespace SnD.ApiClient.Beast.Base;

public interface IBeastClient
{
    /// <summary>
    /// Submit a job to the Beast instance
    /// </summary>
    /// <param name="jobParams"></param>
    /// <param name="submissionConfigurationName"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="concurrencyStrategy">defaults to IGNORE</param>
    /// <returns></returns>
    /// <exception cref="ConcurrencyError">If there is already a run with the same tag and ConcurrencyStrategy is set to <see cref="ConcurrencyStrategy.SKIP"/></exception>
    public Task<RequestState> SubmitJobAsync(JobRequest jobParams, string submissionConfigurationName,
        CancellationToken cancellationToken, ConcurrencyStrategy? concurrencyStrategy);
    
    
    /// <summary>
    /// Awaits a run until it completes with any result or runs out of time set via cancellationToken.
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="pollInterval"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<RequestState> AwaitRunAsync(string requestId, TimeSpan pollInterval, CancellationToken cancellationToken);

    /// <summary>
    /// Get the state of a Beast job
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<RequestState> GetJobStateAsync(string requestId, CancellationToken cancellationToken);
}
