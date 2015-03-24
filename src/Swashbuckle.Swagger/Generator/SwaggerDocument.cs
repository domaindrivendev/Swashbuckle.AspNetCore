using System.Collections.Generic;
using Newtonsoft.Json;

namespace Swashbuckle.Swagger.Generator
{
    public class SwaggerDocument
    {
        public readonly string swagger = "2.0";

        public Info info;

        public string host;

        public string basePath;

        public IList<string> schemes;

        public IList<string> consumes;

        public IList<string> produces;

        public IDictionary<string, PathItem> paths;

        public IDictionary<string, Schema> definitions;

        public IDictionary<string, Parameter> parameters;

        public IDictionary<string, Response> responses;

        public IDictionary<string, SecurityScheme> securityDefinitions;

        public IList<IDictionary<string, IEnumerable<string>>> security;

        public IList<Tag> tags;

        public ExternalDocs externalDocs;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class Info
    {
        public string version;

        public string title;

        public string description;

        public string termsOfService;

        public Contact contact;

        public License license;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class Contact
    {
        public string name;

        public string url;

        public string email;
    }

    public class License
    {
        public string name;

        public string url;
    }

    public class PathItem
    {
        [JsonProperty("$ref")]
        public string @ref;

        public Operation get;

        public Operation put;

        public Operation post;

        public Operation delete;

        public Operation options;

        public Operation head;

        public Operation patch;

        public IList<Parameter> parameters;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class Operation
    {
        public IList<string> tags;

        public string summary;

        public string description;

        public ExternalDocs externalDocs;

        public string operationId;

        public IList<string> consumes;

        public IList<string> produces;

        public IList<Parameter> parameters;

        public IDictionary<string, Response> responses;

        public IList<string> schemes;

        public bool deprecated;

        public IList<IDictionary<string, IEnumerable<string>>> security;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class Tag
    {
        public string name;

        public string description;

        public ExternalDocs externalDocs;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class ExternalDocs
    {
        public string description;

        public string url;
    }

    public class Parameter : PartialSchema
    {
        public string name;

        public string @in;

        public string description;

        public bool required;

        public Schema schema;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class Schema
    {
        [JsonProperty("$ref")]
        public string @ref;

        public string format;

        public string title;

        public string description;

        public object @default;

        public int? multipleOf;

        public int? maximum;

        public bool? exclusiveMaximum;

        public int? minimum;

        public bool? exclusiveMinimum;

        public int? maxLength;

        public int? minLength;

        public string pattern;

        public int? maxItems;

        public int? minItems;

        public bool? uniqueItems;

        public int? maxProperties;

        public int? minProperties;

        public IList<string> required;

        public IList<object> @enum;

        public string type;

        public Schema items;

        public IList<Schema> allOf;

        public IDictionary<string, Schema> properties;

        public Schema additionalProperties;

        public string discriminator;

        public bool? readOnly;

        public Xml xml;

        public ExternalDocs externalDocs;

        public object example;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }

    public class PartialSchema
    {
        public string type;

        public string format; 

        public PartialSchema items;

        public string collectionFormat;

        public object @default;

        public int? maximum;

        public bool? exclusiveMaximum;

        public int? minimum;

        public bool? exclusiveMinimum;

        public int? maxLength;

        public int? minLength;

        public string pattern;

        public int? maxItems;

        public int? minItems;

        public bool? uniqueItems;

        public IList<object> @enum;

        public int? multipleOf;
    }

    public class Response
    {
        public string description;

        public Schema schema;

        public IList<Header> headers;

        public object examples;
    }

    public class Header : PartialSchema
    {
        public string description;
    }

    public class Xml
    {
        public string name;

        public string @namespace;

        public string prefix;

        public bool? attribute;

        public bool? wrapped;
    }

    public class SecurityScheme
    {
        public string type;

        public string description;

        public string name;

        public string @in;

        public string flow;

        public string authorizationUrl;

        public string tokenUrl;

        public IDictionary<string, string> scopes;

        [JsonExtensionData]
        public Dictionary<string, object> vendorExtensions = new Dictionary<string, object>();
    }
}