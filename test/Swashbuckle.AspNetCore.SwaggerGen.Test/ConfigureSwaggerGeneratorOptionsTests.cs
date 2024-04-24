using System.Reflection;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class ConfigureSwaggerGeneratorOptionsTests
{
    [Fact]
    public static void DeepCopy_Copies_All_Properties()
    {
        var type = typeof(SwaggerGeneratorOptions);
        var publicProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // If this assertion fails, it means that a new property has been added
        // to SwaggerGeneratorOptions and ConfigureSwaggerGeneratorOptions.DeepCopy() needs to be updated
        Assert.Equal(18, publicProperties.Length);
    }
}
