using System.Net;
using System.Text.Json;
using SnD.ApiClient.Boxer.Exceptions;
using SnD.ApiClient.Boxer.Models;

namespace SnD.ApiClient.Boxer.Extensions;

public static class BoxerJwtClaimExtensions 
{
    public static async Task<IEnumerable<BoxerJwtClaim>> ExtractClaimsAsync(this HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => throw new UserNotFoundException(),
            _ => JsonSerializer.Deserialize<GetUserClaimsResponse>(await response.EnsureSuccessStatusCode().Content
                .ReadAsStringAsync()).Claims.Select(c => new BoxerJwtClaim(c.First().Key, c.First().Value))
        };
    }
}
