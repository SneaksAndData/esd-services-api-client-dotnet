using System.Text;
using System.Text.Json;
using ESD.ApiClient.Boxer.Base;
using ESD.ApiClient.Crystal.Models;
using ESD.ApiClient.Crystal.Models.Base;

namespace ESD.ApiClient.Crystal;

public class CrystalConnector : Base.ApiClient, ICrystalConnector
{
    private readonly Uri baseUri;

    /// <summary>
    /// Creates new instance
    /// </summary>
    /// <param name="baseUri">Crystal instance URI</param>
    /// <param name="apiVersion">Crystal API version</param>
    /// <param name="httpClient">Http client</param>
    /// <param name="boxerTokenProvider">Boxer token provider instance</param>
    public CrystalConnector(Uri baseUri, string apiVersion, HttpClient httpClient,
        IBoxerTokenProvider boxerTokenProvider) : base(httpClient, boxerTokenProvider)
    {
        this.baseUri = new Uri(baseUri, new Uri(apiVersion, UriKind.Relative));
    }

    /// <inheritdoc/>
    public async Task<CreateRunResponse?> CreateRunAsync(string algorithm, JsonElement payload,
        AlgorithmConfiguration customConfiguration, Func<Task<string>> getTokenAsync,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestUri = new Uri(baseUri, new Uri(algorithm, UriKind.Relative));
        var algorithmRequest = new AlgorithmRequest()
        {
            AlgorithmParameters = payload,
            CustomConfiguration = customConfiguration
        };
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Content =
            new StringContent(JsonSerializer.Serialize(algorithmRequest), Encoding.UTF8, "application/json");
        var response = SendBoxerAuthenticatedRequestAsync(request, getTokenAsync, cancellationToken);
        response.Result.EnsureSuccessStatusCode();
        var responseString = await response.Result.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CreateRunResponse>(responseString);
    }
}