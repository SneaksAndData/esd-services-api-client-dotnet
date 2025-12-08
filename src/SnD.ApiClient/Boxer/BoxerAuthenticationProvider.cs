using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using SnD.ApiClient.Boxer.Base;

namespace SnD.ApiClient.Boxer;

public class BoxerAuthenticationProvider: IAuthenticationProvider
{
    private readonly IJwtTokenExchangeProvider tokenProvider;

    public BoxerAuthenticationProvider(IJwtTokenExchangeProvider tokenProvider)
    {
        this.tokenProvider = tokenProvider;
    }
    
    public async Task AuthenticateRequestAsync(RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        var token = await this.tokenProvider.GetTokenAsync(true, cancellationToken);
        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}