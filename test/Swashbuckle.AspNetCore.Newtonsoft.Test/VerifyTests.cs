using Microsoft.OpenApi;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test;

public class VerifyTests
{
    [Fact]
    public async Task GenerateSchema_UseAllOfForInheritance_WithNonPublicJsonProperties()
    {
        var generatorOptions = new SchemaGeneratorOptions { UseAllOfForInheritance = true };
        var subject = new SchemaGenerator(
            generatorOptions,
            new NewtonsoftDataContractResolver(new JsonSerializerSettings()));
        var schemaRepository = new SchemaRepository();

        subject.GenerateSchema(typeof(NonPublicJsonPropertyType), schemaRepository);

        var document = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = schemaRepository.Schemas
            }
        };

        using var stringWriter = new StringWriter();
        document.SerializeAs(OpenApiSpecVersion.OpenApi3_0, new OpenApiJsonWriter(stringWriter));

        await Verifier.Verify(stringWriter.ToString())
                      .UseDirectory($"snapshots/{Environment.Version.Major}_{Environment.Version.Minor}");
    }
}
