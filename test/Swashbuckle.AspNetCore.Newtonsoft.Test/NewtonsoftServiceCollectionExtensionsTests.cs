using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public static class NewtonsoftServiceCollectionExtensionsTests
{
    [Fact]
    public static async Task AddSwaggerGenNewtonsoftSupport_CreatesSchemaFilter_WhenItDependsOnDataContractResolver()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSwaggerGen((options) => options.SchemaFilter<DataContractResolverSchemaFilter>());
        services.AddSwaggerGenNewtonsoftSupport();

        var provider = services.BuildServiceProvider();

        // A dependency cycle through the options hangs instead of throwing, so resolve
        // them on another thread to fail the test with a timeout.
        var options = await Task.Run(() => provider.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value)
            .WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.IsType<DataContractResolverSchemaFilter>(Assert.Single(options.SchemaFilters));
        await provider.DisposeAsync();
    }

    private sealed class DataContractResolverSchemaFilter(ISerializerDataContractResolver resolver) : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context) => _ = resolver;
    }
}
