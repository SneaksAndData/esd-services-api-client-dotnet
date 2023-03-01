using System;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using SnD.ApiClient.Extensions;

namespace SnD.ApiClient.Azure;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Boxer authorization service using Azure AD provider to DI with custom scopes.
    /// </summary>
    /// <param name="services">Services collection</param>
    /// <param name="scopes">Scopes requested for the Azure cloud token issued to the user. If null is provided, default "https://management.core.windows.net/.default" will be used. </param>
    /// <returns>Services collection</returns>
    public static IServiceCollection AuthorizeWithBoxerOnAzure(this IServiceCollection services, string[] scopes)
    {
        return services.AddBoxerAuthorization(async cancellationToken =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (scopes is { Length: > 0 })
            {
                var credential = new DefaultAzureCredential();
                var context = new TokenRequestContext(scopes: scopes);
                return (await credential.GetTokenAsync(context, cancellationToken)).Token;
            }

            throw new ArgumentOutOfRangeException(nameof(scopes), "Scopes array cannot be null or empty");

        });
    }
    
    /// <summary>
    /// Add Boxer authorization service using Azure AD provider to DI with default scope.
    /// </summary>
    /// <param name="services">Services collection</param>
    /// <returns>Services collection</returns>
    public static IServiceCollection AuthorizeWithBoxerOnAzure(this IServiceCollection services)
    {
        return services.AuthorizeWithBoxerOnAzure(scopes: new [] { "https://management.core.windows.net/.default" });
    }
}