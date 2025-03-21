using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

public class InterceptorFunctions
{
    /// <summary>
    /// MUST be a valid Javascript function.
    /// Function to intercept remote definition, "Try it out", and OAuth 2.0 requests.
    /// Accepts one argument requestInterceptor(request) and must return the modified request, or a Promise that resolves to the modified request.
    /// Ex: "function (req) { req.headers['MyCustomHeader'] = 'CustomValue'; return req; }"
    /// </summary>
    [JsonPropertyName("RequestInterceptorFunction")]
    public string RequestInterceptorFunction { get; set; }

    /// <summary>
    /// MUST be a valid Javascript function.
    /// Function to intercept remote definition, "Try it out", and OAuth 2.0 responses.
    /// Accepts one argument responseInterceptor(response) and must return the modified response, or a Promise that resolves to the modified response.
    /// Ex: "function (res) { console.log(res); return res; }"
    /// </summary>
    [JsonPropertyName("ResponseInterceptorFunction")]
    public string ResponseInterceptorFunction { get; set; }
}
