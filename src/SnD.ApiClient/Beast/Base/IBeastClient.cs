using SnD.ApiClient.Beast.Models;

namespace SnD.ApiClient.Beast.Base;

public interface IBeastClient
{
    /// <summary>
    /// Submit a job to the Beast instance
    /// </summary>
    /// <param name="jobParams"></param>
    /// <param name="submissionConfigurationName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<RequestState> SubmitJobAsync(JobRequest jobParams, string submissionConfigurationName,
        CancellationToken cancellationToken);

    /// <summary>
    /// Get the state of a Beast job
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<RequestState> GetJobStateAsync(string requestId, CancellationToken cancellationToken);
}
