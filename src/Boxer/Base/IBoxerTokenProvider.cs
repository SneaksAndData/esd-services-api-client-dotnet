namespace ESD.ApiClient.Boxer.Base;

public interface IBoxerTokenProvider
{
    /// <summary>
    /// Authenticates with external Authorization provider and return Boxer token
    /// </summary>
    /// <param name="refresh">True if token expired and should be refreshed</param>
    /// <param name="getTokenAsync">Returns external Authorization provider access token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boxer JWT value</returns>
   public Task<string> GetTokenAsync(bool refresh, Func<Task<string>> getTokenAsync,
      CancellationToken cancellationToken);
}