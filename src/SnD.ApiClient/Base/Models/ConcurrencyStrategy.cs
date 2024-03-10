namespace SnD.ApiClient.Base.Models;

/// <summary>
/// The strategy for handling concurrency based on client tag.
/// 1. IGNORE - Ignore the job if a similar job is already running.
/// 2. SKIP - Skip the new job if a similar job is already running.
/// 3. AWAIT - Wait for the similar job to complete before starting the new job.
/// 4. REPLACE - Cancel any similar job and start the new job.
/// </summary>
public enum ConcurrencyStrategy
{
    IGNORE,
    SKIP,
    AWAIT,
    REPLACE
}
