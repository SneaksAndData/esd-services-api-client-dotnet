using System.Text.Json;
using SnD.ApiClient.Boxer.Models;

namespace SnD.ApiClient.Boxer.Base;

public interface IBoxerClient
{
    public Task<IEnumerable<BoxerJwtClaim>> GetClaimsByUserIdAsync(string userId, string provider, CancellationToken cancellationToken);
    
    public Task<bool> PatchClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
    
    public Task<bool> DeleteClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
}
