Installation
============

1. Install the ``Swashbuckle.AspNetCore`` metapackage into your ASP.NET Core application:

    .. code-block:: bash

        > dotnet add package Swashbuckle.AspNetCore

    or via Package Manager ...

    .. code-block:: bash

        > Install-Package Swashbuckle.AspNetCore
    
2. In the ``ConfigureServices`` method of ``Startup.cs``, register the Swagger/OpenAPI generator

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Startup.cs
        :language: csharp
        :start-at: services.AddSwaggerGen
        :lines: 1
        :dedent: 12
    
3. Ensure your API actions and parameters are decorated with explicit "Http" and "From" bindings.

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpPost]
        :end-at: public
        :dedent: 8

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpGet("search")]
        :end-at: public
        :dedent: 8

    *NOTE: If you omit the explicit parameter bindings, the generator will describe them as "query" params by default.*

4. In the ``Configure`` method, insert middleware to expose the generated Swagger/OpenAPI document as a JSON endpoint

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Startup.cs
        :language: csharp
        :start-at: app.UseSwagger
        :lines: 1
        :dedent: 12

    *At this point, you can spin up your application and view the generated Swagger/OpenAPI JSON at "/swagger/v1/swagger.json."*

5. Optionally, if you want to expose interactive documentation, insert the Swagger UI middleware.

    .. literalinclude:: ..\..\..\test\WebSites\GettingStarted\Startup.cs
        :language: csharp
        :start-at: app.UseSwaggerUI
        :lines: 1
        :dedent: 12

    *Now you can restart your application and check out the auto-generated, interactive docs at "/swagger".*