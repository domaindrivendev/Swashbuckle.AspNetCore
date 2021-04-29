using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIOptions
    {
        /// <summary>
        /// Gets or sets a route prefix for accessing the swagger-ui
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        /// <summary>
        /// Gets or sets a Stream function for retrieving the swagger-ui page
        /// </summary>
        public Func<Stream> IndexStream { get; set; } = () => typeof(SwaggerUIOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("Swashbuckle.AspNetCore.SwaggerUI.index.html");

        /// <summary>
        /// Gets or sets a title for the swagger-ui page
        /// </summary>
        public string DocumentTitle { get; set; } = "Swagger UI";

        /// <summary>
        /// Gets or sets additional content to place in the head of the swagger-ui page
        /// </summary>
        public string HeadContent { get; set; } = "";

        /// <summary>
        /// Gets the JavaScript config object, represented as JSON, that will be passed to the SwaggerUI
        /// </summary>
        public ConfigObject ConfigObject  { get; set; } = new ConfigObject();

        /// <summary>
        /// Gets the JavaScript config object, represented as JSON, that will be passed to the initOAuth method
        /// </summary>
        public OAuthConfigObject OAuthConfigObject { get; set; } = new OAuthConfigObject();

        /// <summary>
        /// Gets the interceptor functions that define client-side request/response interceptors
        /// </summary>
        public InterceptorFunctions Interceptors { get; set; } = new InterceptorFunctions();
    }

    public class ConfigObject
    {
        /// <summary>
        /// One or more Swagger JSON endpoints (url and name) to power the UI
        /// </summary>
        public IEnumerable<UrlDescriptor> Urls { get; set; } = null;

        /// <summary>
        /// If set to true, enables deep linking for tags and operations
        /// </summary>
        public bool DeepLinking { get; set; } = false;

        /// <summary>
        /// Controls the display of operationId in operations list
        /// </summary>
        public bool DisplayOperationId { get; set; } = false;

        /// <summary>
        /// The default expansion depth for models (set to -1 completely hide the models)
        /// </summary>
        public int DefaultModelsExpandDepth { get; set; } = 1;

        /// <summary>
        /// The default expansion depth for the model on the model-example section
        /// </summary>
        public int DefaultModelExpandDepth { get; set; } = 1;

        /// <summary>
        /// Controls how the model is shown when the API is first rendered.
        /// (The user can always switch the rendering for a given model by clicking the 'Model' and 'Example Value' links)
        /// </summary>
        public ModelRendering DefaultModelRendering { get; set; } = ModelRendering.Example;

        /// <summary>
        /// Controls the display of the request duration (in milliseconds) for Try-It-Out requests
        /// </summary>
        public bool DisplayRequestDuration { get; set; } = false;

        /// <summary>
        /// Controls the default expansion setting for the operations and tags.
        /// It can be 'list' (expands only the tags), 'full' (expands the tags and operations) or 'none' (expands nothing)
        /// </summary>
        public DocExpansion DocExpansion { get; set; } = DocExpansion.List;

        /// <summary>
        /// If set, enables filtering. The top bar will show an edit box that you can use to filter the tagged operations
        /// that are shown. Can be an empty string or specific value, in which case filtering will be enabled using that
        /// value as the filter expression. Filtering is case sensitive matching the filter expression anywhere inside the tag
        /// </summary>
        public string Filter { get; set; } = null;

        /// <summary>
        /// If set, limits the number of tagged operations displayed to at most this many. The default is to show all operations
        /// </summary>
        public int? MaxDisplayedTags { get; set; } = null;

        /// <summary>
        /// Controls the display of vendor extension (x-) fields and values for Operations, Parameters, and Schema
        /// </summary>
        public bool ShowExtensions { get; set; } = false;

        /// <summary>
        /// Controls the display of extensions (pattern, maxLength, minLength, maximum, minimum) fields and values for Parameters
        /// </summary>
        public bool ShowCommonExtensions { get; set; } = false;

        /// <summary>
        /// OAuth redirect URL
        /// </summary>
        [JsonPropertyName("oauth2RedirectUrl")]
        public string OAuth2RedirectUrl { get; set; } = null;

        /// <summary>
        /// List of HTTP methods that have the Try it out feature enabled.
        /// An empty array disables Try it out for all operations. This does not filter the operations from the display
        /// </summary>
        public IEnumerable<SubmitMethod> SupportedSubmitMethods { get; set; } = Enum.GetValues(typeof(SubmitMethod)).Cast<SubmitMethod>();

        /// <summary>
        /// Controls whether the "Try it out" section should be enabled by default.
        /// </summary>
        [JsonPropertyName("tryItOutEnabled")]
        public bool TryItOutEnabled { get; set; }

        /// <summary>
        /// By default, Swagger-UI attempts to validate specs against swagger.io's online validator.
        /// You can use this parameter to set a different validator URL, for example for locally deployed validators (Validator Badge).
        /// Setting it to null will disable validation
        /// </summary>
        public string ValidatorUrl { get; set; } = null;

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalItems { get; set; } = new Dictionary<string, object>();
    }

    public class UrlDescriptor
    {
        public string Url { get; set; }

        public string Name { get; set; }
    }

    public enum ModelRendering
    {
        Example,
        Model
    }

    public enum DocExpansion
    {
        List,
        Full,
        None
    }

    public enum SubmitMethod
    {
        Get,
        Put,
        Post,
        Delete,
        Options,
        Head,
        Patch,
        Trace
    }

    public class OAuthConfigObject
    {
        /// <summary>
        /// Default clientId
        /// </summary>
        public string ClientId { get; set; } = null;

        /// <summary>
        /// Default clientSecret
        /// </summary>
        public string ClientSecret { get; set; } = null;

        /// <summary>
        /// Realm query parameter (for oauth1) added to authorizationUrl and tokenUrl
        /// </summary>
        public string Realm { get; set; } = null;

        /// <summary>
        /// Application name, displayed in authorization popup
        /// </summary>
        public string AppName { get; set; } = null;

        /// <summary>
        /// Scope separator for passing scopes, encoded before calling, default value is a space (encoded value %20)
        /// </summary>
        public string ScopeSeparator { get; set; } = " ";

        /// <summary>
        /// String array of initially selected oauth scopes, default is empty array
        /// </summary>
        public IEnumerable<string> Scopes { get; set; } = new string[] { };

        /// <summary>
        /// Additional query parameters added to authorizationUrl and tokenUrl
        /// </summary>
        public Dictionary<string, string> AdditionalQueryStringParams { get; set; } = null;

        /// <summary>
        /// Only activated for the accessCode flow. During the authorization_code request to the tokenUrl,
        /// pass the Client Password using the HTTP Basic Authentication scheme
        /// (Authorization header with Basic base64encode(client_id + client_secret))
        /// </summary>
        public bool UseBasicAuthenticationWithAccessCodeGrant { get; set; } = false;

        /// <summary>
        /// Only applies to authorizatonCode flows. Proof Key for Code Exchange brings enhanced security for OAuth public clients.
        /// The default is false
        /// </summary>
        public bool UsePkceWithAuthorizationCodeGrant { get; set; } = false;
    }

    public class InterceptorFunctions
    {
        /// <summary>
        /// MUST be a valid Javascript function.
        /// Function to intercept remote definition, "Try it out", and OAuth 2.0 requests.
        /// Accepts one argument requestInterceptor(request) and must return the modified request, or a Promise that resolves to the modified request.
        /// Ex: "req => { req.headers['MyCustomHeader'] = 'CustomValue'; return req; }"
        /// </summary>
        public string RequestInterceptorFunction { get; set; }

        /// <summary>
        /// MUST be a valid Javascript function.
        /// Function to intercept remote definition, "Try it out", and OAuth 2.0 responses.
        /// Accepts one argument responseInterceptor(response) and must return the modified response, or a Promise that resolves to the modified response.
        /// Ex: "res => { console.log(res); return res; }"
        /// </summary>
        public string ResponseInterceptorFunction { get; set; }
    }
}