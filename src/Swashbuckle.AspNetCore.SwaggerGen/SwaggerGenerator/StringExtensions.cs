namespace Swashbuckle.AspNetCore.SwaggerGen;

internal static class StringExtensions
{
    internal static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var camelCasedParts = value
            .Split('.')
            .Select(part => char.ToLowerInvariant(part[0]) + part[1..]);

        return string.Join('.', camelCasedParts);
    }
}
