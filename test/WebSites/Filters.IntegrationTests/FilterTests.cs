using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.ApiTesting.Xunit;
using Xunit;

namespace Filters.IntegrationTests
{
    public class FilterTests : ApiTestFixture<Startup>
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public FilterTests(WebApplicationFactory<Startup> webApplicationFactory)
            : base(null, webApplicationFactory, "v1")
        {
            _webApplicationFactory = webApplicationFactory;
        }

        [Theory]
        [InlineData("v1", true)]
        [InlineData("v2", false)]
        public async Task DocumentNameCanBeUsedInFilters(string documentName, bool expectAlteredDoc)
        {
            using (var client = _webApplicationFactory.CreateClient())
            {
                // Get the swagger document
                var response = await client.GetAsync(new Uri($"/swagger/{documentName}/swagger.json", UriKind.Relative));
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var json = await response.Content.ReadAsStringAsync();
                dynamic jobj = JObject.Parse(json);

                // v1 document should have messages appended to the descriptions
                // v2 document should have unaltered descriptions

                // IDocumentFilter
                string apiDescription = jobj.info.description;
                Assert.Equal(expectAlteredDoc, apiDescription.EndsWith("<br/><br/><b>This API will be removed, please update to the v2 API.</b>"));

                // IOperationFilter
                string operationDescription = jobj.paths["/products"].get.description;
                Assert.Equal(expectAlteredDoc, operationDescription.EndsWith("<br/><br/><b>This operation will be removed, please update to the v2 API.</b>"));

                // IParameterFilter
                string parameterDescription = jobj.paths["/products"].get.parameters[0].description;
                Assert.Equal(expectAlteredDoc, parameterDescription.EndsWith("<br/><br/><b>This parameter will be removed, please update to the v2 API.</b>"));

                // IRequestBodyFilter
                string requestBodyDescription = jobj.paths["/products"].post.requestBody.description;
                Assert.Equal(expectAlteredDoc, requestBodyDescription.EndsWith("<br/><br/><b>This request body will be removed, please update to the v2 API.</b>"));

                // ISchemaFilter
                string schemaDescription = jobj.components.schemas.Product.description;
                Assert.Equal(expectAlteredDoc, schemaDescription.EndsWith("<br/><br/><b>This schema will be removed, please update to the v2 API.</b>"));
            }
        }
    }
}