using ESD.ApiClient.Boxer;
using Newtonsoft.Json;

namespace ESD.ApiClient.Crystal;

public class CrystalConnector: Base.ApiClient
{
    private readonly Uri baseUri;
    private readonly string apiVersion;

    public CrystalConnector(
        HttpClient httpClient,
        IBoxerTokenProvider boxerTokenProvider,
        Uri baseUri,
        string apiVersion
        ) : base(httpClient, boxerTokenProvider)
    {
        this.baseUri = baseUri;
        this.apiVersion = apiVersion;
    }

    public async Task<CreateRunResponse?> CreateRun(string algorithm, 
        Func<Task<string>> getTokenAsync,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, this.apiVersion);
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        var response = SendBoxerAuthenticatedRequestAsync(request, getTokenAsync, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        return JsonConvert.DeserializeObject<CreateRunResponse>(await response.Result.Content.ReadAsStringAsync());
    }

}