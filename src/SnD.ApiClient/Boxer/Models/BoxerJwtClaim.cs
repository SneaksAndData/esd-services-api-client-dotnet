using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using SnD.ApiClient.Boxer.Extensions;

namespace SnD.ApiClient.Boxer.Models;

[ExcludeFromCodeCoverage]
public class BoxerJwtClaim : Claim
{
    internal BoxerJwtClaim(string type, string value) : base(type, value)
    {
        ApiMethods = value.ToBoxerHttpMethods().ToHashSet();
        Path = type;
    }

    /// <summary>
    /// Static method to convert a Boxer API Claims response to a collection of BoxerJwtClaim objects
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static IEnumerable<BoxerJwtClaim> FromBoxerClaimsApiResponse(string response)
    {
        return JsonSerializer.Deserialize<GetUserClaimsResponse>(response)
        .Claims.Select(c => new BoxerJwtClaim(c.First().Key, c.First().Value));
    }

    /// <summary>
    /// Static constructor to create a BoxerJwtClaim from a path and a collection of ApiMethodElement
    /// </summary>
    /// <param name="path"></param>
    /// <param name="apiMethods"></param>
    /// <returns></returns>
    public static BoxerJwtClaim Create(string path, HashSet<HttpMethod> apiMethods)
    {
        return new BoxerJwtClaim(path, apiMethods.ToRegexString());
    }

    /// <summary>
    /// API path
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// API methods
    /// </summary>
    public readonly HashSet<HttpMethod> ApiMethods;
}
