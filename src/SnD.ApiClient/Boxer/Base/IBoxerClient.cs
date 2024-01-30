using System.Text.Json;
using SnD.ApiClient.Boxer.Models;

namespace SnD.ApiClient.Boxer.Base;

public interface IBoxerClient
{
    /// <summary>
    /// Create a jwt-user registration in Boxer for a given user id and provider
    /// If user already exists, the method not do anything and return true
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if success or if user already exists</returns>
    public Task<bool> CreateUserAsync(string userId, string provider, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get claims by user id and provider
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerator of object <see cref="BoxerJwtClaim"/> for the user</returns>
    public Task<IEnumerable<BoxerJwtClaim>> GetClaimsByUserIdAsync(string userId, string provider, CancellationToken cancellationToken);
    
    /// <summary>
    /// Set (Update/Edit) claims for user id and provider
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="claims">Claims to set in the form of an enumerator of type <see cref="BoxerJwtClaim"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the operation was successful, false otherwise</returns>
    public Task<bool> PatchClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete claims for user id and provider
    /// Note: The method will match claims by user, identity provider and path. It will not match by api methods.
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="claims">Claims to delete in the form of an enumerator of type <see cref="BoxerJwtClaim"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<bool> DeleteClaimsByUserIdAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
}
