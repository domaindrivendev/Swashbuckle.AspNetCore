using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

public static class SwaggerGenOptionsExtensions
{
    public static void Configure(SwaggerGenOptions options)
    {
        // begin-snippet: SwaggerGen-DocInclusionPredicate
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
            {
                return false;
            }

            var versions = methodInfo.DeclaringType?
                .GetCustomAttributes(true)
                .OfType<ApiVersionAttribute>()
                .SelectMany(attribute => attribute.Versions) ?? [];

            return versions.Any(version => $"v{version}" == docName);
        });
        // end-snippet
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class ApiVersionAttribute : Attribute
{
    public List<string> Versions { get;set; } = [];
}
