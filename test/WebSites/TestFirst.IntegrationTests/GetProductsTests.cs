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
    public class GetProductsTests : ApiTestFixture<TestFirst.Startup>
    {
        public GetProductsTests(
            ApiTestRunner apiTestRunner,
            WebApplicationFactory<TestFirst.Startup> webApplicationFactory)
            : base(apiTestRunner, webApplicationFactory, "v1-imported")
        { }

        [Fact]
        public async Task GetProducsts_Returns200_IfRequiredParametersProvided()
        {
            await TestAsync(
                "GetProducts",
                "200",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products?pageNo=1", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }

        [Fact]
        public async Task GetProducts_Returns400_IfRequiredParametersMissing()
        {
            await TestAsync(
                "GetProducts",
                "400",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }
    }
}