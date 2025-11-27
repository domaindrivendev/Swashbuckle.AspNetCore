using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

public static class IServiceCollectionExtensions
{
    public static void Configure(this IServiceCollection services)
    {
        // begin-snippet: README-Newtonsoft.Json
        services.AddMvc();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });

        services.AddSwaggerGenNewtonsoftSupport();
        // end-snippet

        // begin-snippet: README-MvcCore
        services.AddMvcCore()
                .AddApiExplorer();
        // end-snippet

        // begin-snippet: Annotations-Enable
        services.AddSwaggerGen(options =>
        {
            // Other setup, then...
            options.EnableAnnotations();
        });
        // end-snippet

        // begin-snippet: Annotations-EnablePolymorphism
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations(enableAnnotationsForInheritance: true, enableAnnotationsForPolymorphism: true);
        });
        // end-snippet

        // begin-snippet: Swagger-CustomSerializerServices
        services.ConfigureSwagger(options =>
        {
            options.SetCustomDocumentSerializer<CustomDocumentSerializer>();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-CustomNamingStrategyConfiguration
        services.AddSwaggerGen(options =>
        {
            // Other configuration...

            // Use method name as operationId
            options.CustomOperationIds(apiDescription =>
            {
                return apiDescription.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
            });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-ConfigureXmlDocumentation
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "My API - V1",
                    Version = "v1"
                }
            );

            options.IncludeXmlComments(Assembly.GetExecutingAssembly());
            // or options.IncludeXmlComments(typeof(MyController).Assembly));
        });
        // end-snippet

        // begin-snippet: SwaggerGen-GlobalMetadata
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "My API - V1",
                    Version = "v1",
                    Description = "A sample API to demo Swashbuckle",
                    TermsOfService = new Uri("http://tempuri.org/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Joe Developer",
                        Email = "joe.developer@tempuri.org"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
                    }
                }
            );
        });
        // end-snippet

        // begin-snippet: SwaggerGen-MultipleDocuments
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API - V1", Version = "v1" });
            options.SwaggerDoc("v2", new OpenApiInfo { Title = "My API - V2", Version = "v2" });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-ConfigureControllerModelConvention
        services.AddMvc(options =>
            options.Conventions.Add(new ApiExplorerGroupPerVersionConvention())
        );
        // end-snippet

        // begin-snippet: SwaggerGen-IgnoreObsoleteActions
        services.AddSwaggerGen(options =>
        {
            options.IgnoreObsoleteActions();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-IgnoreObsoleteProperties
        services.AddSwaggerGen(options =>
        {
            options.IgnoreObsoleteProperties();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-HiddenByConventionConfiguration
        services.AddMvc(options =>
            options.Conventions.Add(new ApiExplorerGetsOnlyConvention())
        );
        // end-snippet

        // begin-snippet: SwaggerGen-CustomTags
        services.AddSwaggerGen(options =>
        {
            options.TagActionsBy(api => [api.HttpMethod]);
        });
        // end-snippet

        // begin-snippet: SwaggerGen-CustomSorting
        services.AddSwaggerGen(options =>
        {
            options.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
        });
        // end-snippet

        // begin-snippet: SwaggerGen-CustomSchemaIds
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds((type) => type.FullName);
        });
        // end-snippet

        // begin-snippet: SwaggerGen-CustomSchemaMapping
        services.AddSwaggerGen(options =>
        {
            options.MapType<PhoneNumber>(() => new OpenApiSchema { Type = JsonSchemaType.String });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-ConfigureOperationFilter
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<AuthResponsesOperationFilter>();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-ConfigureSchemaFilter
        services.AddSwaggerGen(options =>
        {
            options.SchemaFilter<AutoRestSchemaFilter>();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-ConfigureSchemaFilterForEnumDictionaryEnum
        services.AddSwaggerGen(options =>
        {
            // These will be replaced by DictionaryTKeyEnumTValueSchemaFilter, but are needed to avoid
            // an error. You will need one for every kind of Dictionary<,> you have.
            options.MapType<Dictionary<MyEnum, List<string>>>(() => new OpenApiSchema());
            options.SchemaFilter<DictionaryTKeyEnumTValueSchemaFilter>();
        });
        // end-snippet

        // begin-snippet: SwaggerGen-AddSecurityDefinition
        services.AddSwaggerGen(options =>
        {
            // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                        Scopes = new Dictionary<string, string>
                        {
                            ["readAccess"] = "Access read operations",
                            ["writeAccess"] = "Access write operations"
                        }
                    }
                }
            });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-AddSecurityRequirement
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
            {
                [new OpenApiSecuritySchemeReference("oauth2", document)] = ["readAccess", "writeAccess"]
            });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-BearerAuthentication
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme."
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-DetectSubtypes
        services.AddSwaggerGen(options =>
        {
            options.UseAllOfForInheritance();

            options.SelectSubTypesUsing(baseType =>
            {
                return typeof(Program).Assembly.GetTypes().Where(type => type.IsSubclassOf(baseType));
            });
        });
        // end-snippet

        // begin-snippet: SwaggerGen-UseAllOfForInheritance
        services.AddSwaggerGen(options =>
        {
            options.UseAllOfForInheritance();

            options.SelectDiscriminatorNameUsing((baseType) => "TypeName");
            options.SelectDiscriminatorValueUsing((subType) => subType.Name);
        });
        // end-snippet
    }
}
