using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

public class ConfigObject
{
    /// <summary>
    /// One or more Swagger JSON endpoints (url and name) to power the UI
    /// </summary>
    [JsonPropertyName("urls")]
    public IEnumerable<UrlDescriptor> Urls { get; set; }

    /// <summary>
    /// If set to true, enables deep linking for tags and operations
    /// </summary>
    [JsonPropertyName("deepLinking")]
    public bool DeepLinking { get; set; }

    /// <summary>
    /// If set to true, it persists authorization data and it would not be lost on browser close/refresh
    /// </summary>
    [JsonPropertyName("persistAuthorization")]
    public bool PersistAuthorization { get; set; }

    /// <summary>
    /// Controls the display of operationId in operations list
    /// </summary>
    [JsonPropertyName("displayOperationId")]
    public bool DisplayOperationId { get; set; }

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
#if NET
    [JsonConverter(typeof(JavascriptStringEnumConverter<ModelRendering>))]
#endif
    [JsonPropertyName("defaultModelRendering")]
    public ModelRendering DefaultModelRendering { get; set; } = ModelRendering.Example;

    /// <summary>
    /// Controls the display of the request duration (in milliseconds) for Try-It-Out requests
    /// </summary>
    [JsonPropertyName("displayRequestDuration")]
    public bool DisplayRequestDuration { get; set; }

    /// <summary>
    /// Controls the default expansion setting for the operations and tags.
    /// It can be 'list' (expands only the tags), 'full' (expands the tags and operations) or 'none' (expands nothing)
    /// </summary>
#if NET
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
    public string Filter { get; set; }

    /// <summary>
    /// If set, limits the number of tagged operations displayed to at most this many. The default is to show all operations
    /// </summary>
    [JsonPropertyName("maxDisplayedTags")]
    public int? MaxDisplayedTags { get; set; }

    /// <summary>
    /// Controls the display of vendor extension (x-) fields and values for Operations, Parameters, and Schema
    /// </summary>
    [JsonPropertyName("showExtensions")]
    public bool ShowExtensions { get; set; }

    /// <summary>
    /// Controls the display of extensions (pattern, maxLength, minLength, maximum, minimum) fields and values for Parameters
    /// </summary>
    [JsonPropertyName("showCommonExtensions")]
    public bool ShowCommonExtensions { get; set; }

    /// <summary>
    /// OAuth redirect URL
    /// </summary>
    [JsonPropertyName("oauth2RedirectUrl")]
    public string OAuth2RedirectUrl { get; set; }

    /// <summary>
    /// List of HTTP methods that have the Try it out feature enabled.
    /// An empty array disables Try it out for all operations. This does not filter the operations from the display
    /// </summary>
#if NET
    [JsonConverter(typeof(JavascriptStringEnumEnumerableConverter<SubmitMethod>))]
#endif
    [JsonPropertyName("supportedSubmitMethods")]
    public IEnumerable<SubmitMethod> SupportedSubmitMethods { get; set; } =
#if NET
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
    public string ValidatorUrl { get; set; }

    /// <summary>
    /// Any custom plugins' function names.
    /// </summary>
    [JsonPropertyName("plugins")]
    public IList<string>  Plugins { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object> AdditionalItems { get; set; } = [];
}
