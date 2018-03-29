using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Swashbuckle.AspNetCore.AzureFunctions.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

using FunctionApp.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.AzureFunctions.Filters;

namespace FunctionApp
{
    public static class HelloWorldFunction
    {
        [FunctionName("HelloWorldFunction")]
        [SwaggerOperation("helloworld")]
        [SwaggerResponse((int)HttpStatusCode.OK, typeof(GreetingsResponseModel), "Returns greetings")]
        [OptionalQueryParameter("lastname", typeof(string))]
        [SupportedMediaType("application/json")]
        [SupportedMediaType("application/xml")]
        [SupportedMediaType("text/xml")]
        [SwaggerOperationFilter(typeof(AppendHttpMethodToOperationIdFilter))]
        [SwaggerOperationFilter(typeof(RemoveBodyParameterFromGetFilter))]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "hello/{firstname}")]
            [RequestBodyType(typeof(MessageRequestModel), "Post person to greeting")]
            HttpRequestMessage req,
            string firstname,
            TraceWriter log)
        {
            // Get request body
            var message = await req.Content.ReadAsAsync<MessageRequestModel>();

            // Get from query
            var lastname = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "lastname", true) == 0)
                .Value;

            // parse query parameter
            var greetingsResponseModel = new GreetingsResponseModel()
            {
                Firstname = firstname,
                Lastname = lastname,
                Message = message?.Message ?? $"Hello {firstname} {lastname}"
            };

            return req.CreateResponse(HttpStatusCode.OK, greetingsResponseModel);
        }
    }
}
