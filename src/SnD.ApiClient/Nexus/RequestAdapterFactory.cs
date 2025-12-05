using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Nexus;

public static class RequestAdapterFactory
{
    
    public static IRequestAdapter ToRequestAdapter(this NexusClientOptions options)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(options.BaseUri);
        var authProvider = new AnonymousAuthenticationProvider();
        return new HttpClientRequestAdapter(authProvider, httpClient: httpClient);
    }
    
}