namespace Swashbuckle.Swagger
{
    internal static class StringExtensions
    {
        internal static string ToCamelCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Substring(0, 1).ToLowerInvariant() + value.Substring(1);
        }
    }
}
