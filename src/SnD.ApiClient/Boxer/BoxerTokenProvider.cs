using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using SnD.ApiClient.Boxer.Base;
using SnD.ApiClient.Config;

namespace SnD.ApiClient.Boxer;

public class BoxerTokenProvider : IJwtTokenExchangeProvider
{
   private readonly Uri authProvider;
   private readonly Uri baseUri;
   private string token;
   private readonly HttpClient httpClient;
   private readonly ILogger logger;
   private readonly Func<CancellationToken, Task<string>> getExternalTokenAsync;

   /// <summary>
   /// Creates new instance
   /// </summary>
   /// <param name="boxerTokenProviderOptions">Options for boxer connector</param>
   /// <param name="httpClient">Http client</param>
   /// <param name="logger">Logger for boxer connector</param>
   /// <param name="getExternalTokenAsync">Function that returns external authentication token</param>
   public BoxerTokenProvider(IOptions<BoxerTokenProviderOptions> boxerTokenProviderOptions, HttpClient httpClient,
      ILogger<BoxerTokenProvider> logger, Func<CancellationToken, Task<string>> getExternalTokenAsync)
   {
      var authorizationProvider = boxerTokenProviderOptions.Value.IdentityProvider
                                  ?? throw new ArgumentNullException(nameof(BoxerTokenProviderOptions.IdentityProvider));
      this.authProvider = new Uri($"token/{authorizationProvider}", UriKind.Relative);
      this.baseUri = new Uri(boxerTokenProviderOptions.Value.BaseUri
                             ?? throw new ArgumentException(nameof(BoxerTokenProviderOptions.BaseUri)));
      this.httpClient = httpClient;
      this.logger = logger;
      this.getExternalTokenAsync = getExternalTokenAsync;
   }

   /// <inheritdoc/>
   public async Task<string> GetTokenAsync(bool refresh, CancellationToken cancellationToken)
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
               var externalToken = await this.getExternalTokenAsync(cancellationToken);
               this.logger.LogInformation("Received external authentication token");
               var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUri, authProvider));
               request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", externalToken);
               return await httpClient.SendAsync(request, cancellationToken);
            });
         this.token = await response.Content.ReadAsStringAsync();
         this.logger.LogInformation("Received boxer token");
      }
      return this.token;
   }
}