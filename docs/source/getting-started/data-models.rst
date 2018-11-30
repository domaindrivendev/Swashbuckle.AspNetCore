Data Models (Schemas)
===========================

In OpenAPI 3.0, the data types exposed by an API are described using an extended subset of the `JSON Schema Specification Wright Draft 00 (aka Draft 5) <https://tools.ietf.org/html/draft-wright-json-schema-00#section-4.2>`_.

Parameters, request body and response payloads can all be assigned a ``schema`` instance to describe their data structure. For a given schema instance, the ``type`` keyword indicates the data type that it represents and can be set to ``string``, ``number``, ``integer``, ``boolean``, ``array`` or ``object``. Schemas can be defined inline or they can reference a shared definition from the ``components.schemas`` section of the OpenAPI document. To learn more about the use of JSON Schema's in the OpenAPI Specification, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/data-models/>`_.

When generating ``parameters``, a ``requestBody``, or ``responses`` for an operation, Swashbuckle will automatically generate a corresponding schema according to the model type and your application's serialization settings. For simple types (e.g. ``string``, ``int`` etc.), it will define the schema inline whereas for user-defined reference types (e.g. ``User``) it will define the schema in the ``components.schemas`` section of the OpenAPI document and reference the definition via the ``$ref`` keyword.

For example, consider the following action that accepts a number of parameters from the request query string, and returns an object that will be serialized to the response payload.

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpGet("search")]
        :end-at: public
        :dedent: 8

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Models\PagingParams.cs
        :language: csharp
        :start-at: public class PagingParams
        :dedent: 4

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Models\User.cs
        :language: csharp
        :start-at: public class User
        :dedent: 4

For the parameters, Swashbuckle will generate the schemas according to ASP.NET Core's `model binding behavior <https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-3.1>`_, and for the response it will generate the schema according to the `JSON serializer behavior <https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to>`_:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\swagger.yaml
        :language: yaml
        :start-after: operationId: SearchUsers
        :end-before: x-end: SearchUsers
        :dedent: 6

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\swagger.yaml
        :language: yaml
        :start-at: components:
        :end-before: x-end: components

.. note:: By default, Swashbuckle will honor the behavior of the System.Text.Json (STJ) serializer that ships with ASP.NET Core. If you're using the `Newtonsoft serializer <https://www.newtonsoft.com/json/help/html/SerializationGuide.htm>`_, then you'll need to install an additional package and explicitly opt-in for Swashbuckle to honor it's behavior instead. See :doc:`../serializer-support` for more info.