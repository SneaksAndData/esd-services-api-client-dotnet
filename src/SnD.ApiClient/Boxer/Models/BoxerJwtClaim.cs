using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

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
        return JsonSerializer.Deserialize<GetUserClaimsResponse>(response)
        .Claims.Select(c => new BoxerJwtClaim(c.First().Key, c.First().Value));
    }

    /// <summary>
    /// Static constructor to create a BoxerJwtClaim from a path and a collection of ApiMethodElement
    /// </summary>
    /// <param name="path"></param>
    /// <param name="apiMethods"></param>
    /// <returns></returns>
    public static BoxerJwtClaim Create(string path, HashSet<ApiMethodElement> apiMethods)
    {
        // Should be ^(GET|POST)$ for example for GET and POST, should be .* for all
        var value = apiMethods.Count == Enum.GetValues(typeof(ApiMethodElement)).Length
            ? ".*"
            : $"^({string.Join("|", apiMethods.Select(v => v.ToString()))})$";
        return new BoxerJwtClaim(path, value);
    }

    /// <summary>
    /// API path
    /// </summary>
    public string Path => Type;

    /// <summary>
    /// API methods
    /// </summary>
    public HashSet<ApiMethodElement> ApiMethods => ParseClaims(Value);

    
    /// <summary>
    /// Parse the value of the claim to a collection of ApiMethodElement
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static HashSet<ApiMethodElement> ParseClaims(string value)
    {
        if (value == ".*")
        {
            return new HashSet<ApiMethodElement>(Enum.GetValues(typeof(ApiMethodElement)).Cast<ApiMethodElement>());
        }
        if(Regex.Match(value, @"\((.*)\)").Groups.Count > 1)
        {
            return new HashSet<ApiMethodElement>(
                Regex.Match(value, @"\((.*)\)").Groups[1].Value.Split('|')
                    .Select(v => Enum.Parse(typeof(ApiMethodElement), v, true))
                    .Cast<ApiMethodElement>());
        }
        if (Enum.TryParse<ApiMethodElement>(value, true, out var apiMethod))
        {
            return new HashSet<ApiMethodElement> { apiMethod };
        }
        return new HashSet<ApiMethodElement>();
    }
}
