namespace SnD.ApiClient.Beast.Models;

public enum BeastRequestLifeCycleStage
{
    NEW,
    QUEUED,
    ALLOCATING,
    ALLOCATED,
    SUBMITTING,
    RUNNING,
    SCHEDULING_FAILED,
    SUBMISSION_FAILED,
    FAILED,
    COMPLETED,
    STALE,
    RETRY
}
