using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore;
using Swashbuckle.AspNetCore.ApiTesting.Xunit;
using Xunit;

namespace TestFirst.IntegrationTests;

public class CreateUserTests : ApiTestFixture<Startup>
{
    public CreateUserTests(
        ApiTestRunner apiTestRunner,
        WebApplicationFactory<Startup> webApplicationFactory)
        : base(apiTestRunner, webApplicationFactory, "v1-generated")
    {
        Describe("/api/users", HttpMethod.Post, new OpenApiOperation
        {
            OperationId = "CreateUser",
            RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [ "application/json" ] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaTypes.Object,
                            Properties = new Dictionary<string, IOpenApiSchema>
                            {
                                [ "email" ] = new OpenApiSchema { Type = JsonSchemaTypes.String },
                                [ "password" ] = new OpenApiSchema { Type = JsonSchemaTypes.String },
                            },
                            Required = [ "email", "password" ]
                        }
                    }
                },
                Required = true
            },
            Responses = new OpenApiResponses
            {
                [ "201" ] = new OpenApiResponse
                {
                    Description = "User created",
                    Headers = new Dictionary<string, IOpenApiHeader>
                    {
                        [ "Location" ] = new OpenApiHeader
                        {
                            Required = true,
                            Schema = new OpenApiSchema { Type = JsonSchemaTypes.String }
                        }
                    }
                },
                [ "400" ] = new OpenApiResponse
                {
                    Description = "Invalid request"
                }
            }
        });
    }

    [Fact]
    public async Task CreateUser_Returns201_IfContentIsValid()
    {
        await TestAsync(
            "CreateUser",
            "201",
            new HttpRequestMessage
            {
                RequestUri = new Uri("/api/users", UriKind.Relative),
                Method = HttpMethod.Post,
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { email = "foo@bar.com", password = "pass123" }),
                    Encoding.UTF8,
                    "application/json")
            }
        );
    }

    [Fact]
    public async Task CreateUser_Returns400_IfContentIsInValid()
    {
        await TestAsync(
            "CreateUser",
            "400",
            new HttpRequestMessage
            {
                RequestUri = new Uri("/api/users", UriKind.Relative),
                Method = HttpMethod.Post,
                Content = new StringContent(
                    JsonConvert.SerializeObject(new { email = "foo@bar.com" }),
                    Encoding.UTF8,
                    "application/json")
            }
        );
    }
}
