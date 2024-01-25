using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;

namespace SnD.ApiClient.Boxer.Models;

[ExcludeFromCodeCoverage]
public class BoxerJwtClaim : Claim
{
    private BoxerJwtClaim(string type, string value) : base(type, value)
    {
    }

    /// <summary>
    /// Static method to convert a Boxer API Claims response to a collection of BoxerJwtClaim objects
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public static IEnumerable<BoxerJwtClaim> FromBoxerClaimsApiResponse(string response)
    {
        var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
        return claims.Select(c => new BoxerJwtClaim(c.Key, c.Value));
    }

    /// <summary>
    /// Static constructor to create a BoxerJwtClaim from a path and a collection of ApiMethodElement
    /// </summary>
    /// <param name="path"></param>
    /// <param name="apiMethods"></param>
    /// <returns></returns>
    public static BoxerJwtClaim Create(string path, HashSet<ApiMethodElement> apiMethods)
    {
        var value = string.Join(",",
            apiMethods.Count == Enum.GetNames(typeof(ApiMethodElement)).Length
                ? ".*"
                : apiMethods.Select(v => v.ToString()));
        return new BoxerJwtClaim(path, value);
    }

    /// <summary>
    /// API path
    /// </summary>
    public string Path => Type;

    /// <summary>
    /// API methods
    /// </summary>
    public HashSet<ApiMethodElement> ApiMethods =>
        new(
            (Value == ".*"
                ? Enum.GetNames(typeof(ApiMethodElement))
                : Value.Split(','))
            .Select(v => Enum.Parse(typeof(ApiMethodElement), v, true))
            .Cast<ApiMethodElement>()
        );
}
