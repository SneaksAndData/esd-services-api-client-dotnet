using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;


namespace SnD.ApiClient.Boxer.Models;

[ExcludeFromCodeCoverage]
internal class BoxerClaimsApiPatchBody
{
    private BoxerClaimsApiPatchBody()
    {
    }

    public static BoxerClaimsApiPatchBody CreateInsertOperation(IEnumerable<BoxerJwtClaim> claims) =>
        new()
        {
            Operation = "Insert",
            Claims = claims.ToDictionary(c => c.Type, c => c.Value)
        };

    public static BoxerClaimsApiPatchBody CreateDeleteOperation(IEnumerable<BoxerJwtClaim> claims) =>
        new()
        {
            Operation = "Delete",
            Claims = claims.ToDictionary(c => c.Type, c => c.Value)
        };

    [JsonPropertyName("operation")]
    public string Operation { get; private set; }

    [JsonPropertyName("claims")]
    public Dictionary<string, string> Claims { get; private set; } = new();
}
