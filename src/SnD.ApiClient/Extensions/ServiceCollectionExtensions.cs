using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SnD.ApiClient.Beast;
using SnD.ApiClient.Beast.Base;
using SnD.ApiClient.Boxer;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;
using SnD.ApiClient.Crystal;
using SnD.ApiClient.Crystal.Base;
using SnD.ApiClient.Nexus;
using SnD.ApiClient.Nexus.Base;

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
    /// Add Boxer authorization service to DI
    /// </summary>
    /// <param name="services">Services collection</param>
    /// <returns>Services collection</returns>
    public static IServiceCollection AuthorizeWithBoxerOnKubernetes(this IServiceCollection services)
    {
        return services.AddBoxerAuthorization(cancellationToken =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            return Task.FromResult(File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token"));
        });
    }
    
    /// <summary>
    /// Add Crystal connector to DI
    /// </summary>
    public static IServiceCollection AddCrystalClient(this IServiceCollection services)
    {
        return services.AddSingleton<ICrystalClient, CrystalClient>();
    }
    
    /// <summary>
    /// Add Boxer Claims Client to DI
    /// </summary>
    public static IServiceCollection AddBoxerClaimsClient(this IServiceCollection services)
    {
        return services.AddSingleton<IBoxerClaimsClient, BoxerClaimsClient>();
    }
    
    /// <summary>
    /// Add Beast Client to DI
    /// </summary>
    public static IServiceCollection AddBeastClient(this IServiceCollection services)
    {
        return services.AddSingleton<IBeastClient, BeastClient>();
    }
    
    
    /// <summary>
    /// Add Authentication provider to DI
    /// </summary>
    public static IServiceCollection AddAuthenticationProvider(this IServiceCollection services)
    {
        return services.AddSingleton<IAuthenticationProvider, BoxerAuthenticationProvider>();
    }

    /// <summary>
    /// Add Nexus Client to DI
    /// </summary>
    public static IServiceCollection AddNexusClient(this IServiceCollection services)
    {
        return services.AddSingleton<INexusClient, NexusClient>();
    }
    
    /// <summary>
    /// Add Nexus Client to DI
    /// </summary>
    public static IServiceCollection AddNexusRetryPolicy(this IServiceCollection services, Func<IServiceProvider, RetryAllErrors> retryStrategyFactory)
    {
        return services.AddSingleton<IRequestAdapter>(sp =>
        {
            var authenticationProvider = sp.GetRequiredService<IAuthenticationProvider>();
            var nexusOptions = sp.GetRequiredService<IOptions<NexusClientOptions>>().Value;
            var retryStrategy = retryStrategyFactory(sp);
            var retryOption = retryStrategy.ToRetryHandlerOption();
            var httpClient = KiotaClientFactory.Create(optionsForHandlers: [retryOption]);
            httpClient.BaseAddress = new Uri(nexusOptions.BaseUri);
            var baseAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient);
            return baseAdapter;
        });
    }
}
