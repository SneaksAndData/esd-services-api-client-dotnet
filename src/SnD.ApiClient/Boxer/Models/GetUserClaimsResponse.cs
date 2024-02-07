using System.Text.Json.Serialization;

namespace SnD.ApiClient.Boxer.Models;

internal class GetUserClaimsResponse
{
    [JsonPropertyName("identityProvider")]
    public string IdentityProvider { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    [JsonPropertyName("claims")]
    public List<Dictionary<string, string>> Claims { get; set; }

    [JsonPropertyName("billingId")]
    public object BillingId { get; set; }
}
