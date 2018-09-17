using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.ApiTesting.Xunit;
using Xunit;

namespace TestFirst.IntegrationTests
{
    public class CreateProductTests : ApiTestFixture<TestFirst.Startup>
    {
        public CreateProductTests(
            ApiTestRunner apiTestRunner,
            WebApplicationFactory<TestFirst.Startup> webApplicationFactory)
            : base(apiTestRunner, webApplicationFactory, "v1-imported")
        { }

        [Fact]
        public async Task CreateProduct_Returns201_IfContentIsValid()
        {
            await TestAsync(
                "CreateProduct",
                "201",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new { name = "foobar" }),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }

        [Fact]
        public async Task CreateProduct_Returns400_IfContentIsInValid()
        {
            await TestAsync(
                "CreateProduct",
                "400",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        JsonConvert.SerializeObject(new { }),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }
    }
}