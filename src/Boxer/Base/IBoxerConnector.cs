namespace ESD.ApiClient.Boxer.Base;

public interface IJwtTokenExchangeProvider
{
    /// <summary>
    /// Authenticates with external Authorization provider and return Boxer token
    /// </summary>
    /// <param name="refresh">True if token expired and should be refreshed</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Boxer JWT value</returns>
   public Task<string> GetTokenAsync(bool refresh, CancellationToken cancellationToken);
}