using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI;

public class OAuthConfigObject
{
    /// <summary>
    /// Default username for OAuth2 password flow.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Default clientId
    /// </summary>
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; }

    /// <summary>
    /// Default clientSecret
    /// </summary>
    /// <remarks>Setting this exposes the client secrets in inline javascript in the swagger-ui generated html.</remarks>
    [JsonPropertyName("clientSecret")]
    public string ClientSecret { get; set; }

    /// <summary>
    /// Realm query parameter (for oauth1) added to authorizationUrl and tokenUrl
    /// </summary>
    [JsonPropertyName("realm")]
    public string Realm { get; set; }

    /// <summary>
    /// Application name, displayed in authorization popup
    /// </summary>
    [JsonPropertyName("appName")]
    public string AppName { get; set; }

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
    public Dictionary<string, string> AdditionalQueryStringParams { get; set; }

    /// <summary>
    /// Only activated for the accessCode flow. During the authorization_code request to the tokenUrl,
    /// pass the Client Password using the HTTP Basic Authentication scheme
    /// (Authorization header with Basic base64encode(client_id + client_secret))
    /// </summary>
    [JsonPropertyName("useBasicAuthenticationWithAccessCodeGrant")]
    public bool UseBasicAuthenticationWithAccessCodeGrant { get; set; }

    /// <summary>
    /// Only applies to authorizatonCode flows. Proof Key for Code Exchange brings enhanced security for OAuth public clients.
    /// The default is false
    /// </summary>
    [JsonPropertyName("usePkceWithAuthorizationCodeGrant")]
    public bool UsePkceWithAuthorizationCodeGrant { get; set; }
}
