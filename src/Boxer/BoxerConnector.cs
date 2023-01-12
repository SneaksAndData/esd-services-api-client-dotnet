using System.Net.Http.Headers;
using ESD.ApiClient.Boxer.Base;
using Polly;

namespace ESD.ApiClient.Boxer;

public class BoxerConnector : IBoxerTokenProvider
{
   private readonly Uri authProvider;
   private readonly Uri baseUri;
   private string? token;
   private readonly HttpClient httpClient;

   /// <summary>
   /// Creates new instance
   /// </summary>
   /// <param name="baseUri">Crystal instance URI</param>
   /// <param name="authProvider">External Authorization provider name</param>
   /// <param name="httpClient">Http client</param>
   public BoxerConnector(Uri baseUri, string authProvider, HttpClient httpClient)
   {
      this.authProvider = new Uri(authProvider, UriKind.Relative);
      this.baseUri = baseUri;
      this.httpClient = httpClient;
   }

   /// <inheritdoc/>
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