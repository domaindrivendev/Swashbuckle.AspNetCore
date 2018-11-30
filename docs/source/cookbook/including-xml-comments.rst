Including XML Comments
======================

The various definitions (operations, parameters, responses, schemas etc.) in an OpenAPI document can include a ``description`` field, and in the case of operations - an additional ``summary`` field, to enrich the docs with human readable content.

With Swashbuckle, you can provide these values by annotating actions, classes and properties with a supported subset of  `XML Comments <https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc>`_ tags. To enable this feature, follow the steps below:

1. Configure your project to output an XML Comments file at buildtime:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\XmlComments.csproj
        :language: xml
        :start-at: <PropertyGroup>
        :end-at: </PropertyGroup>
        :dedent: 2

    *NOTE: Supressing the "1591" warning is not neccessary here but can be useful if you're using XML Comments for Swashbuckle only*.

2. Configure Swashbuckle to incorporate one or more XML Comments file(s) into the generated OpenAPI document:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\Startup.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :end-before: services.ConfigureSwaggerGen
        :dedent: 12

Now, you can start annotating your actions and properties to include additional descriptive text in the generated document.

Operations, Params & Responses
------------------------------

To enrich operation, parameter and response definitions with human readable descriptions, you can annotate action methods with the familiar ``<summary>``, ``<remarks>`` and ``<param>`` tags, as well as the Swashbuckle-specific ``<response>`` tag. For example, given the following action method:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\Controllers\UsersController.cs
        :language: csharp
        :start-at: /// <summary>
        :end-at: public
        :dedent: 8
 
Swashbuckle will generate the following operation:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\swagger.yaml
        :language: yaml
        :start-after: /users
        :end-before: x-end: CreateUser
        :dedent: 4

Schemas & Properties
--------------------

To enrich schema and property definitions, you can annotate both classes and properties with the ``<summary>`` tag. For example, given the following class:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\Models\User.cs
        :language: csharp
        :start-at: /// <summary>
        :dedent: 4
 
Swashbuckle will generate the following schema:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\swagger.yaml
        :language: yaml
        :start-at: User:
        :end-before: UserStatus:
        :dedent: 4

.. note:: By default, Swashbuckle does NOT include descriptions for "reference" schemas. This is because, as stated in the `JSON Schema spec <https://tools.ietf.org/html/draft-pbryan-zyp-json-ref-03#section-3>`_, the ``$ref`` field should not be accompanied by other fields. You can workaround this by using the ``allOf`` keyword to "extend" the reference schema. See #TODO for more info.

Global Tags
-----------

In OpenAPI, you can assign a list of ``tags`` to each operation for downstream tools and libraries to use as they see fit. For example, the Swagger UI uses ``tags`` to group the displayed operations. Additionally, you can specify a ``description`` for each tag by using the global ``tags`` section on the root document.

By default, Swashbuckle tags operations with the corresponding controller name but does not include global descriptions for those tags. However, you can add these to the generated document by passing the opt-in flag and decorating controllers with the ``<summary>`` tag:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\StartupWithTagDescriptions.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :end-before: services.ConfigureSwaggerGen
        :dedent: 12

Given the following controller:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\Controllers\ProductsController.cs
        :language: csharp
        :start-at: /// <summary>
        :end-at: public
        :dedent: 4

Swashbuckle will generate the following tags section on the document root:

    .. literalinclude:: ..\..\..\test\WebSites\XmlComments\swagger-with-tag-descriptions.yaml
        :language: yaml
        :start-after: x-end: components