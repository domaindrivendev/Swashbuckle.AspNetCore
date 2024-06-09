﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
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

        /// <summary>
        /// Gets or sets the optional JSON serialization options to use to serialize options to the HTML document.
        /// </summary>
        public JsonSerializerOptions JsonSerializerOptions { get; set; }

        /// <summary>
        /// Gets or sets the path or URL to the Swagger UI JavaScript bundle file.
        /// </summary>
        public string ScriptBundlePath { get; set; } = "./swagger-ui-bundle.js";

        /// <summary>
        /// Gets or sets the path or URL to the Swagger UI JavaScript standalone presets file.
        /// </summary>
        public string ScriptPresetsPath { get; set; } = "./swagger-ui-standalone-preset.js";

        /// <summary>
        /// Gets or sets the path or URL to the Swagger UI CSS file.
        /// </summary>
        public string StylesPath { get; set; } = "./swagger-ui.css";
    }

    public class ConfigObject
    {
        /// <summary>
        /// One or more Swagger JSON endpoints (url and name) to power the UI
        /// </summary>
        [JsonPropertyName("urls")]
        public IEnumerable<UrlDescriptor> Urls { get; set; } = null;

        /// <summary>
        /// If set to true, enables deep linking for tags and operations
        /// </summary>
        [JsonPropertyName("deepLinking")]
        public bool DeepLinking { get; set; } = false;

        /// <summary>
        /// If set to true, it persists authorization data and it would not be lost on browser close/refresh
        /// </summary>
        [JsonPropertyName("persistAuthorization")]
        public bool PersistAuthorization { get; set; } = false;

        /// <summary>
        /// Controls the display of operationId in operations list
        /// </summary>
        [JsonPropertyName("displayOperationId")]
        public bool DisplayOperationId { get; set; } = false;

        /// <summary>
        /// The default expansion depth for models (set to -1 completely hide the models)
        /// </summary>
        [JsonPropertyName("defaultModelsExpandDepth")]
        public int DefaultModelsExpandDepth { get; set; } = 1;

        /// <summary>
        /// The default expansion depth for the model on the model-example section
        /// </summary>
        [JsonPropertyName("defaultModelExpandDepth")]
        public int DefaultModelExpandDepth { get; set; } = 1;

        /// <summary>
        /// Controls how the model is shown when the API is first rendered.
        /// (The user can always switch the rendering for a given model by clicking the 'Model' and 'Example Value' links)
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonConverter(typeof(JavascriptStringEnumConverter<ModelRendering>))]
#endif
        [JsonPropertyName("defaultModelRendering")]
        public ModelRendering DefaultModelRendering { get; set; } = ModelRendering.Example;

        /// <summary>
        /// Controls the display of the request duration (in milliseconds) for Try-It-Out requests
        /// </summary>
        [JsonPropertyName("displayRequestDuration")]
        public bool DisplayRequestDuration { get; set; } = false;

        /// <summary>
        /// Controls the default expansion setting for the operations and tags.
        /// It can be 'list' (expands only the tags), 'full' (expands the tags and operations) or 'none' (expands nothing)
        /// </summary>
#if NET6_0_OR_GREATER
        [JsonConverter(typeof(JavascriptStringEnumConverter<DocExpansion>))]
#endif
        [JsonPropertyName("docExpansion")]
        public DocExpansion DocExpansion { get; set; } = DocExpansion.List;

        /// <summary>
        /// If set, enables filtering. The top bar will show an edit box that you can use to filter the tagged operations
        /// that are shown. Can be an empty string or specific value, in which case filtering will be enabled using that
        /// value as the filter expression. Filtering is case sensitive matching the filter expression anywhere inside the tag
        /// </summary>
        [JsonPropertyName("filter")]
        public string Filter { get; set; } = null;

        /// <summary>
        /// If set, limits the number of tagged operations displayed to at most this many. The default is to show all operations
        /// </summary>
        [JsonPropertyName("maxDisplayedTags")]
        public int? MaxDisplayedTags { get; set; } = null;

        /// <summary>
        /// Controls the display of vendor extension (x-) fields and values for Operations, Parameters, and Schema
        /// </summary>
        [JsonPropertyName("showExtensions")]
        public bool ShowExtensions { get; set; } = false;

        /// <summary>
        /// Controls the display of extensions (pattern, maxLength, minLength, maximum, minimum) fields and values for Parameters
        /// </summary>
        [JsonPropertyName("showCommonExtensions")]
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
#if NET6_0_OR_GREATER
        [JsonConverter(typeof(JavascriptStringEnumEnumerableConverter<SubmitMethod>))]
#endif
        [JsonPropertyName("supportedSubmitMethods")]
        public IEnumerable<SubmitMethod> SupportedSubmitMethods { get; set; } =
#if NET5_0_OR_GREATER
            Enum.GetValues<SubmitMethod>();
#else
            Enum.GetValues(typeof(SubmitMethod)).Cast<SubmitMethod>();
#endif

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
        [JsonPropertyName("validatorUrl")]
        public string ValidatorUrl { get; set; } = null;

        [JsonExtensionData]
        public Dictionary<string, object> AdditionalItems { get; set; } = [];
    }

    public class UrlDescriptor
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
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
        /// Default username for OAuth2 password flow.
        /// </summary>
        public string Username { get; set; } = null;

        /// <summary>
        /// Default clientId
        /// </summary>
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = null;

        /// <summary>
        /// Default clientSecret
        /// </summary>
        /// <remarks>Setting this exposes the client secrets in inline javascript in the swagger-ui generated html.</remarks>
        [JsonPropertyName("clientSecret")]
        public string ClientSecret { get; set; } = null;

        /// <summary>
        /// Realm query parameter (for oauth1) added to authorizationUrl and tokenUrl
        /// </summary>
        [JsonPropertyName("realm")]
        public string Realm { get; set; } = null;

        /// <summary>
        /// Application name, displayed in authorization popup
        /// </summary>
        [JsonPropertyName("appName")]
        public string AppName { get; set; } = null;

        /// <summary>
        /// Scope separator for passing scopes, encoded before calling, default value is a space (encoded value %20)
        /// </summary>
        [JsonPropertyName("scopeSeparator")]
        public string ScopeSeparator { get; set; } = " ";

        /// <summary>
        /// String array of initially selected oauth scopes, default is empty array
        /// </summary>
        [JsonPropertyName("scopes")]
        public IEnumerable<string> Scopes { get; set; } = [];

        /// <summary>
        /// Additional query parameters added to authorizationUrl and tokenUrl
        /// </summary>
        [JsonPropertyName("additionalQueryStringParams")]
        public Dictionary<string, string> AdditionalQueryStringParams { get; set; } = null;

        /// <summary>
        /// Only activated for the accessCode flow. During the authorization_code request to the tokenUrl,
        /// pass the Client Password using the HTTP Basic Authentication scheme
        /// (Authorization header with Basic base64encode(client_id + client_secret))
        /// </summary>
        [JsonPropertyName("useBasicAuthenticationWithAccessCodeGrant")]
        public bool UseBasicAuthenticationWithAccessCodeGrant { get; set; } = false;

        /// <summary>
        /// Only applies to authorizatonCode flows. Proof Key for Code Exchange brings enhanced security for OAuth public clients.
        /// The default is false
        /// </summary>
        [JsonPropertyName("usePkceWithAuthorizationCodeGrant")]
        public bool UsePkceWithAuthorizationCodeGrant { get; set; } = false;
    }

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
}
