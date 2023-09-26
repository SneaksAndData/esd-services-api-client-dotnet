namespace SnD.ApiClient.Crystal.Models.Base;

/// <summary>
/// Request lifecycle stages. Used by metadata store and maintenance service.
/// </summary>
public enum RequestLifeCycleStage
{
    NEW,
    BUFFERED,
    RUNNING,
    COMPLETED,
    FAILED,
    SCHEDULING_TIMEOUT,
    DEADLINE_EXCEEDED,
    THROTTLED,
    CLIENT_TIMEOUT
}
