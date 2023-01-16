using ESD.ApiClient.Boxer;
using ESD.ApiClient.Boxer.Base;
using ESD.ApiClient.Config;
using ESD.ApiClient.Crystal;
using ESD.ApiClient.Crystal.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ESD.ApiClient.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Boxer authorization service to DI
    /// </summary>
    /// <param name="services">Services collection</param>
    /// <param name="externalTokenFactory">Function that provides external identity provider token</param>
    /// <returns>Services collection</returns>
    public static IServiceCollection AddBoxerAuthorization(this IServiceCollection services,
        Func<CancellationToken, Task<string>> externalTokenFactory)
    {
        services.AddSingleton<IBoxerConnector, BoxerConnector>(sp => new BoxerConnector(
            sp.GetRequiredService<IOptions<BoxerConnectorOptions>>(),
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<ILogger<BoxerConnector>>(),
            externalTokenFactory));
        return services;
    }

    /// <summary>
    /// Add Crystal connector to DI
    /// </summary>
    public static IServiceCollection AddCrystalConnector(this IServiceCollection services)
    {
        services.AddSingleton<ICrystalConnector, CrystalConnector>();
        return services;
    }
}