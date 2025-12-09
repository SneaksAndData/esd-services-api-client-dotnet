using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Nexus;

public static class RequestAdapterFactory
{
    public static IRequestAdapter ToRequestAdapter(this IOptions<NexusClientOptions> options,
        IAuthenticationProvider authenticationProvider,
        Func<HttpClientRequestAdapter, IRequestAdapter> adapterFactory,
        Func<HttpClient> httpClientFactory)
    {
        var httpClient = httpClientFactory();
        httpClient.BaseAddress = new Uri(options.Value.BaseUri);
        return adapterFactory(new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient));
    }
}
