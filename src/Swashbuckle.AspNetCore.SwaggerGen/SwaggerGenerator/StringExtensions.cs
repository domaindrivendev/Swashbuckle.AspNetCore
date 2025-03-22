namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class StringExtensions
{
    internal static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

#if NET
        var cameCasedParts = value
            .Split('.')
            .Select(part => char.ToLowerInvariant(part[0]) + part[1..]);

        return string.Join('.', cameCasedParts);
#else
        var cameCasedParts = value
            .Split('.')
            .Select(part => char.ToLowerInvariant(part[0]) + part.Substring(1));

        return string.Join(".", cameCasedParts);
#endif
    }
}
