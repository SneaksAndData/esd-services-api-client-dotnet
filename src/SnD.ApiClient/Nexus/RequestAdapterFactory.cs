using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Nexus;

public static class RequestAdapterFactory
{
    public static IRequestAdapter ToRequestAdapter(this IOptions<NexusClientOptions> options,
        IAuthenticationProvider authenticationProvider, Func<HttpClient>? httpClientFactory = null)
    {
        var httpClient = httpClientFactory == null ? new HttpClient() : httpClientFactory();
        httpClient.BaseAddress = new Uri(options.Value.BaseUri);
        return new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient);
    }
}