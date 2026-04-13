using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace SnD.ApiClient.Tests.Acceptance.Nexus;

public class EmptyAuthenticationProvider: IAuthenticationProvider
{
    private const string Token = "mockToken";
    
    public Task AuthenticateRequestAsync(RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        request.Headers.Add("Authorization", $"Bearer {Token}");
        return Task.CompletedTask;
    }
}