using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
using SnD.ApiClient.Config;
using SnD.ApiClient.Nexus.Base;

namespace SnD.ApiClient.Nexus;

public class RetryAllErrors : INexusRetryStrategy
{
    private readonly ILogger<RetryAllErrors> logger;
    private readonly NexusClientOptions options;

    public RetryAllErrors(ILogger<RetryAllErrors> logger, IOptions<NexusClientOptions> nexusClientOptions)
    {
        this.logger = logger;
        this.options = nexusClientOptions.Value;
    }
    
    public bool ShouldRetry(int delay, int tryCount, HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return false;
        }
        this.logger.LogWarning(
            "Retry {RetryCount} after {Delay} due to status {Status}: {Message}",
            tryCount,
            delay,
            response.StatusCode,
            response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        );
        return true;
    }
    
    public RetryHandlerOption ToRetryHandlerOption()
    {
        var retryOption = new RetryHandlerOption
        {
            Delay = this.options.RetryIntervalSeconds,
            MaxRetry = this.options.MaxRetryAttempts,
            ShouldRetry = this.ShouldRetry
        };
        return retryOption;
    }
}