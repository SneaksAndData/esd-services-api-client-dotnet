using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace SnD.ApiClient.Nexus.Base;

/// <summary>
/// Defines retry strategy for Nexus client requests
/// </summary>
public interface INexusRetryStrategy
{
    /// <summary>
    /// Returns whether a request should be retried
    /// </summary>
    /// <param name="delay">Retry delay in seconds</param>
    /// <param name="tryCount">The current retry attempt count</param>
    /// <param name="response">The HTTP response message</param>
    /// <returns>True if the request should be retried; otherwise, false</returns>
    bool ShouldRetry(int delay, int tryCount, HttpResponseMessage response);
    
    
    /// <summary>
    /// Converts the strategy to a RetryHandlerOption, consumable by Kiota middleware
    /// </summary>
    RetryHandlerOption ToRetryHandlerOption();
}