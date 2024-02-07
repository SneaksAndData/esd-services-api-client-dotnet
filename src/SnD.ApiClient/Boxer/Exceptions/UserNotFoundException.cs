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
    public UserNotFoundException() : base("Requested user not found in Boxer")
    {
    }
}
