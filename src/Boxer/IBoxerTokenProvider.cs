namespace ESD.ApiClient.Boxer;

public interface IBoxerTokenProvider
{
   public Task<string> GetTokenAsync(bool refresh, Func<Task<string>> getTokenAsync,
      CancellationToken cancellationToken);
}