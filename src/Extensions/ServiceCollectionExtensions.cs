using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal;
using SnD.ApiClient.Crystal.Base;

namespace SnD.ApiClient.Extensions;

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
        services.AddSingleton<IJwtTokenExchangeProvider, BoxerTokenProvider>(sp => new BoxerTokenProvider(
            sp.GetRequiredService<IOptions<BoxerTokenProviderOptions>>(),
            sp.GetRequiredService<HttpClient>(),
            sp.GetRequiredService<ILogger<BoxerTokenProvider>>(),
            externalTokenFactory));
        return services;
    }

    /// <summary>
    /// Add Crystal connector to DI
    /// </summary>
    public static IServiceCollection AddCrystalClient(this IServiceCollection services)
    {
        return services.AddSingleton<ICrystalClient, CrystalClient>();
    }
}