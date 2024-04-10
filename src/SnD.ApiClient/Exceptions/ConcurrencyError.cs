using SnD.ApiClient.Base.Models;

namespace SnD.ApiClient.Exceptions;

/// <summary>
/// Thrown on jobs requested with a concurrency strategy that conflicts with an existing job.
/// </summary>
/// <param name="strategy">Chosen concurrency strategy</param>
/// <param name="existingRequestId">Request id of (one of) the existing job</param>
/// <param name="clientTag">Client tag for concurrency tracking</param>
public class ConcurrencyError(ConcurrencyStrategy strategy, string existingRequestId, string clientTag)
    : Exception(
        $"Concurrent request found with client tag {clientTag}, and request id {existingRequestId}. Chosen strategy: {strategy}")
{
    public ConcurrencyStrategy Strategy { get; } = strategy;
    public string ExistingRequestId { get; } = existingRequestId;
    public string ClientTag { get; } = clientTag;
}
