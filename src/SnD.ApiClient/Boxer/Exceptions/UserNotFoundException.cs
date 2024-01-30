using System.Diagnostics.CodeAnalysis;

namespace SnD.ApiClient.Boxer.Exceptions;

/// <summary>
/// Exception thrown when a user is not found in Boxer, e.g., when trying to add claims to a user that does not exist
/// </summary>
[ExcludeFromCodeCoverage]
public class UserNotFoundException : Exception
{
    /// <summary>
    /// Create a new instance of <see cref="UserNotFoundException"/>
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="provider"></param>
    public UserNotFoundException(string userId, string provider) 
        : base($"User {userId} not found under provider {provider}")
    {
        UserId = userId;
        Provider = provider;
    }

    /// <summary>
    /// User id in Boxer
    /// </summary>
    public string UserId { get; }
    
    /// <summary>
    /// Provider (IDP) in Boxer
    /// </summary>
    public string Provider { get; }
}
