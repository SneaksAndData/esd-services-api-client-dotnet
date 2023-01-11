using System.Net.Http.Headers;
using Polly;

namespace ESD.ApiClient.Boxer;

public class BoxerConnector : IBoxerTokenProvider
{
   private readonly Uri authProvider;
   private readonly Uri baseUri;
   private string? token;
   private readonly HttpClient httpClient;

   public BoxerConnector(Uri baseUri, HttpClient httpClient, string authProvider)
   {
      this.authProvider = new Uri(authProvider, UriKind.Relative);
      this.baseUri = baseUri;
      this.httpClient = httpClient;
   }

   public async Task<string> GetTokenAsync(bool refresh, Func<Task<string>> getTokenAsync, CancellationToken cancellationToken)
   {
      cancellationToken.ThrowIfCancellationRequested();
      if (this.token == null || refresh)
      {
         var response = await Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .ExecuteAsync(async () =>
            {
               var externalToken = await getTokenAsync();
               var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, authProvider));
               request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", externalToken);
               return await httpClient.SendAsync(request, cancellationToken);
            });
         this.token = await response.Content.ReadAsStringAsync();
      }
      return this.token;
   }
}