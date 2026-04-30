using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using SnD.ApiClient.Boxer.Base;

namespace SnD.ApiClient.Boxer;

public class BoxerAuthenticationProvider(IJwtTokenExchangeProvider tokenProvider) : IAuthenticationProvider
{
    public async Task AuthenticateRequestAsync(RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var token = await tokenProvider.GetTokenAsync(true, cancellationToken);
        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}