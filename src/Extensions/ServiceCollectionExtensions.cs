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

    public static IServiceCollection AddCrystalConnector(this IServiceCollection services)
    {
        services.AddSingleton<ICrystalConnector, CrystalConnector>();
        return services;
    }
}