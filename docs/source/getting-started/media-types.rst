Media Types
===========================

For a given API, request and response contents may be transmitted in different *media types* - e.g. ``appliction/json``, ``application/xml`` etc. OpenAPI 3.0 provides the ``content`` field on request body and response descriptions to list supported media types. To learn more about how media types are described by the OpenAPI Specification, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/media-types/>`_.

When generating request and response definitions, Swashbuckle will list supported media types according to the input and output formatters configured for your application. For example, if you're using the ``SystemTextJsonInputFormatter``, then Swashbuckle will include a definition for the following media types on request bodies, because these are the media types explicitly supported by that formatter:

- ``application/json``
- ``text/json``
- ``application/*+json``

It's worth noting however, that ASP.NET Core does provide the ``[Consumes]`` and ``[Produces]`` attributes, which can be applied at the action or controller level, to further constrain the media types supported for a given operation or group of operations. In this case, Swashbuckle will honor the behavior and only list the explicitly supported media types.

For example, given the following controller:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Controllers\UsersController.cs
        :language: csharp
        :start-at: [Consumes
        :end-at: public
        :dedent: 4

Swashbuckle will only generate a single ``application/json`` media type for the relevant request body and response definitions:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\swagger.yaml
        :language: yaml
        :start-after: operationId: CreateUser
        :end-before: responses:
        :dedent: 6

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\swagger.yaml
        :language: yaml
        :start-after: operationId: GetUsers
        :end-before: x-end: GetUsers
        :dedent: 6

.. note:: If you've configured your application to support XML media types (as described `here <https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?#add-xml-format-support>`_), then Swashbuckle will automatically list the additional media type. However, support for honoring ``XmlSerializer`` behavior is currently limited and requires some workarounds to generate accurate schema definitions. See :doc:`../../cookbook/xml-media-types` for more info.