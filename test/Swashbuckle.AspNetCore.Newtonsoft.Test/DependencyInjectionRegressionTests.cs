using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class DependencyInjectionRegressionTests
{
    [Fact]
    public async Task Can_generate_schema_when_schema_filter_requires_serializer_resolver()
    {
        var generation = Task.Run(() =>
        {
            var services = new ServiceCollection();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSwaggerGen(options => options.SchemaFilter<ResolverSchemaFilter>());
            services.AddSwaggerGenNewtonsoftSupport();
            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<SchemaGeneratorOptions>>().Value;
            var resolver = provider.GetRequiredService<ISerializerDataContractResolver>();
            var schema = new SchemaGenerator(options, resolver).GenerateSchema(typeof(TestModel), new SchemaRepository());

            Assert.NotNull(schema);
            Assert.True(Assert.IsType<ResolverSchemaFilter>(Assert.Single(options.SchemaFilters)).Applied);
        }, TestContext.Current.CancellationToken);

        await generation.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
    }

    private sealed class ResolverSchemaFilter(ISerializerDataContractResolver resolver) : ISchemaFilter
    {
        public bool Applied { get; private set; }

        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            Assert.NotNull(resolver.GetDataContractForType(context.Type));
            Applied = true;
        }
    }

    private sealed class TestModel
    {
        public string Value { get; set; }
    }
}
