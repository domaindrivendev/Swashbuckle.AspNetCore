Providing General API Info
===========================

In an OpenAPI document, the ``info`` section can be used to provide general information about an API. It includes a ``title`` and ``version``, which are required, and a range of optional fields such as ``description``, ``termsOfService`` etc. To learn more about this section of the OpenAPI document, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/api-general-info/>`_.

With the default setup, Swashbuckle will generate a single OpenAPI document for your API, with the ``title`` set to to the name of your Startup DLL and the ``version`` to "1.0". To edit these values and/or provide additional info, you can register the document explicitly, and provide an ``OpenApiInfo`` instance:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\StartupWithApiInfo.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :end-before: services.ConfigureSwaggerGen
        :dedent: 12

.. note:: The first parameter to ``SwaggerDoc`` is a unique name for the document, and is significant because it corresponds to the ``{documentName}`` parameter in the URL for retrieving OpenAPI documents as JSON or YAML - e.g. ``/swagger/{documentName}/swagger.json``. With the default setup, the SwaggerUI middleware assumes an OpenAPI document can be found at ``/swagger/v1/swagger.json``. So, if you register the document here with a value other than ``v1``, then you'll need to update the SwaggerUI middleware accordingly. See #TODO for more on this.