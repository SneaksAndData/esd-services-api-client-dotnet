using System.Diagnostics.CodeAnalysis;

namespace SnD.ApiClient.Boxer.Exceptions;

/// <summary>
/// Exception thrown when a user is not found in Boxer, e.g., when trying to add claims to a user that does not exist
/// </summary>
[ExcludeFromCodeCoverage]
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId, string provider)
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
