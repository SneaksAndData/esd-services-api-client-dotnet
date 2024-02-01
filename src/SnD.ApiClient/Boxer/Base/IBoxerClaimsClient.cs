using System.Text.Json;
using SnD.ApiClient.Boxer.Exceptions;
using SnD.ApiClient.Boxer.Models;

namespace SnD.ApiClient.Boxer.Base;

public interface IBoxerClaimsClient
{
    /// <summary>
    /// Create a jwt-user registration in Boxer for a given user id and provider
    /// If user already exists, the method not do anything and return true
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>true if success or if user already exists</returns>
    /// <exception cref="HttpRequestException">Throws if the request to Boxer fails</exception>
    public Task<bool> CreateUserAsync(string userId, string provider, CancellationToken cancellationToken);
    
    /// <summary>
    /// Deletes a user from Boxer by user id and provider (jwt-user registration) and all its claims.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="provider"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="UserNotFoundException">Throws if the user is not found under that identity provider</exception>
    /// <exception cref="HttpRequestException">Throws if the request to Boxer fails</exception>
    public Task<bool> DisassociateUserAsync(string userId, string provider, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get claims by user id and provider
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enumerator of object <see cref="BoxerJwtClaim"/> for the user</returns>
    /// <exception cref="UserNotFoundException">Throws if the user is not found under that identity provider</exception>
    /// <exception cref="HttpRequestException">Throws if the request to Boxer fails</exception>
    public Task<IEnumerable<BoxerJwtClaim>> GetUserClaimsAsync(string userId, string provider, CancellationToken cancellationToken);
    
    /// <summary>
    /// Set (Update/Edit) claims for user id and provider
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="claims">Claims to set in the form of an enumerator of type <see cref="BoxerJwtClaim"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the operation was successful, false otherwise</returns>
    /// <exception cref="UserNotFoundException">Throws if the user is not found under that identity provider</exception>
    /// <exception cref="HttpRequestException">Throws if the request to Boxer fails</exception>
    public Task<bool> PatchUserClaimsAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete claims for user id and provider
    /// Note: The method will match claims by user, identity provider and path. It will not match by api methods.
    /// </summary>
    /// <param name="userId">User principal name (UPN) in Boxer</param>
    /// <param name="provider">Identity provider (IDP) in Boxer</param>
    /// <param name="claims">Claims to delete in the form of an enumerator of type <see cref="BoxerJwtClaim"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>true on success</returns>
    /// <exception cref="UserNotFoundException">Throws if the user is not found under that identity provider</exception>
    /// <exception cref="HttpRequestException">Throws if the request to Boxer fails</exception>
    public Task<bool> DeleteUserClaimsAsync(string userId, string provider, IEnumerable<BoxerJwtClaim> claims, CancellationToken cancellationToken);
}
