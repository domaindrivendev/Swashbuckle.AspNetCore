Describing API Security
=======================

In OpenAPI 3.0, you can describe how an API is secured by defining one or more security schemes (e.g. basic, API key, OAuth2 etc.) and then specifying which of those schemes are required, either globally or for specific operations. To learn more about describing security in an OpenAPI document, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/describing-parameters/>`_.

In Swashbuckle, you can define schemes by invoking the ``AddSecurityDefinition`` method, providing a name and an instance of ``OpenApiSecurityScheme`` that describes the scheme. If the scheme is applicable to all operations, you can invoke the ``AddSecurityRequirements`` method, or alternatively you can wire up an operation filter that applies the scheme to specific operations based on the presence of ``[Authorize]`` attributes on controllers and action methods.

API Key Example
---------------

This example is for an API that requires a valid API key to be provided in the query string for *all* operations. You can describe this scheme with Swashbuckle as follows:

    .. literalinclude:: ..\..\..\test\WebSites\ApiKeySecurity\Startup.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :end-before: services.ConfigureSwaggerGen
        :dedent: 12

With this setup, Swashbuckle will generate the following scheme definition and (global) requirement:

    .. literalinclude:: ..\..\..\test\WebSites\ApiKeySecurity\swagger.yaml
        :language: yaml
        :start-at: securitySchemes:
        :end-before: x-end: components
        :dedent: 2

    .. literalinclude:: ..\..\..\test\WebSites\ApiKeySecurity\swagger.yaml
        :language: yaml
        :start-at: security:

.. note:: If you're using the Swagger UI, it will display authentication popups that can be used in conjuction with the "Try it out" functionality, based on the security schemes and requirements specified in the OpenAPI document.

OAuth2 Example
---------------

This example is for an API that accepts an OAuth2 access token, obtained via the `Authorization Code flow <https://tools.ietf.org/html/rfc6749#section-4.1>`_ , and authorizes operations based on scopes included with the token. To describe this with Swashbuckle, you can define an ``OAuth2`` scheme, and wire up an operation filter that applies the scheme to specific operations based on the presence of ``[Authorize]`` attributes:

    .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\Startup.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :end-before: services.ConfigureSwaggerGen
        :dedent: 12

The filter implementation will depend on how you've implemented authorization within your app. For example, if you're using named policies to encapsulate and apply scope requirements with the ``[Authorize]`` attribute, then you could implement the filter as follows:

    .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\Swagger\OAuthOperationFilter.cs
        :language: csharp
        :start-at: public class OAuthOperationFilter
        :dedent: 4

With this setup, Swashbuckle will generate the following scheme definition:

    .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\swagger.yaml
        :language: yaml
        :start-at: securitySchemes:
        :end-before: x-end: components
        :dedent: 2

And for any operations that have the custom policies applied ...

    .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpPost]
        :end-at: public int CreateUser
        :dedent: 8

It will generate the following operation metadata, including a ``security`` section that references the "oauth2" scheme and scopes required by the policy:

    .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\swagger.yaml
        :language: yaml
        :start-at: operationId: CreateUser
        :end-before: x-end: CreateUser
        :dedent: 6

.. note:: While this setup is a bit more involved, it's extremely powerful, especially when combined with the Swagger UI, as it has built-in support for interactive OAuth2 flows.

    If your OpenAPI document includes OAuth2 definitions and requirements, the interactive flow(s) will be enabled automatically. However, you can further customize OAuth2 support in the UI with the following settings. See `the swagger-ui docs here <https://github.com/swagger-api/swagger-ui/blob/v3.42.0/docs/usage/oauth2.md>`_ for more info:


        .. literalinclude:: ..\..\..\test\WebSites\OAuthSecurity\Startup.cs
            :language: csharp
            :start-at: app.UseSwaggerUI
            :end-before: app.UseRouting
            :dedent: 12