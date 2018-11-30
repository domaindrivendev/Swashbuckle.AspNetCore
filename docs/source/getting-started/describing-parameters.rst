Describing Parameters
========================

In OpenAPI 3.0, parameters are defined in the ``parameters`` section of an ``operation``. The ``in`` keyword is used to indicate the type of parameter and can be set to ``path``, ``query``, ``header`` or ``cookie``. To learn more about how request parameters are described by the OpenAPI Specification, checkout out the `OpenAPI docs here <https://swagger.io/docs/specification/describing-parameters/>`_.

When generating an ``operation`` for an action method, Swashbuckle will automatically generate parameter definitions for any parameters or model properties that are bound to the route, query string or headers collection.

For example, given the following action method:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpGet("search")]
        :end-at: public
        :dedent: 8

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Models\PagingParams.cs
        :language: csharp
        :start-at: public class PagingParams
        :dedent: 4

Swashbuckle will generate the following parameters:

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\swagger.yaml
        :language: yaml
        :start-after: operationId: SearchUsers
        :end-before: responses:
        :dedent: 6