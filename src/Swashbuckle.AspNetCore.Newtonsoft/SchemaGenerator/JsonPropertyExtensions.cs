using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.Newtonsoft;

/// <summary>
/// A class containing extension methods for the <see cref="JsonProperty"/> class. This class cannot be inherited.
/// </summary>
public static class JsonPropertyExtensions
{
    /// <summary>
    /// Tries to get the <see cref="MemberInfo"/> for the given <see cref="JsonProperty"/>.
    /// </summary>
    /// <param name="jsonProperty">The <see cref="JsonProperty"/> to get the <see cref="MemberInfo"/> for.</param>
    /// <param name="memberInfo">When the method returns, contains the <see cref="MemberInfo"/> for the property, if found.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="MemberInfo"/> was found; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryGetMemberInfo(this JsonProperty jsonProperty, out MemberInfo memberInfo)
    {
        memberInfo = jsonProperty.DeclaringType?
            .GetMember(jsonProperty.UnderlyingName)
            .FirstOrDefault();

        return memberInfo != null;
    }

    /// <summary>
    /// Returns whether the specified <see cref="JsonProperty"/> is specified whether it is required.
    /// </summary>
    /// <param name="jsonProperty">The <see cref="JsonProperty"/> to test.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="JsonProperty"/> specifies whether it is required; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsRequiredSpecified(this JsonProperty jsonProperty)
        => jsonProperty.Required != Required.Default;
}
