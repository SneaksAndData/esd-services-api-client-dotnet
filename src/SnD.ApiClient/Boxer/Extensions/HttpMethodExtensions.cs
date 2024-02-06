using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace SnD.ApiClient.Boxer.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Convert a string to a collection of HttpMethod that matches (by regex) the string
    /// </summary>
    /// <param name="httpMethods"></param>
    /// <returns></returns>
    public static IEnumerable<HttpMethod> ToBoxerHttpMethods(this string httpMethods)
    {
        if (string.IsNullOrWhiteSpace(httpMethods))
            return Enumerable.Empty<HttpMethod>();
        var regex = new Regex(httpMethods);
        return new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Patch }
            .Where(m => regex.IsMatch(m.Method));
    }

    /// <summary>
    /// Convert a collection of HttpMethod to a regex string that matches the collection
    /// </summary>
    /// <param name="httpMethods"></param>
    /// <returns></returns>
    public static string ToRegexString(this IEnumerable<HttpMethod> httpMethods)
    {
        var methods = httpMethods.Select(m=>m.Method).ToImmutableSortedSet();
        return methods.IsEmpty ? string.Empty : $"^({string.Join("|", methods)})$";
    }
}
