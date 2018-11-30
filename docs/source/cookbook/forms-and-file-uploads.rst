Forms and File Uploads
===================================

Form data
---------

For example, given the following action method:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\Controllers\UsersController.cs
        :language: csharp
        :start-at: [HttpPost]
        :end-at: public
        :dedent: 8

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\Models\User.cs
        :language: csharp
        :start-at: public class User
        :dedent: 4

Swashbuckle will generate the following request body:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\swagger.yaml
        :language: yaml
        :start-after: operationId: CreateUser
        :end-before: responses:
        :dedent: 6

File Uploads
------------

For example, given the following action method:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\Controllers\ProductsController.cs
        :language: csharp
        :start-at: [HttpPut("{id}/picture")]
        :end-at: public
        :dedent: 8

Swashbuckle will generate the following request body:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\swagger.yaml
        :language: yaml
        :start-after: operationId: UpdatePicture
        :end-before: responses:
        :dedent: 6

Multipart Requests
------------------

For example, given the following action method:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\Controllers\ProductsController.cs
        :language: csharp
        :start-at: [HttpPost]
        :end-at: public
        :dedent: 8

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\Models\Product.cs
        :language: csharp
        :start-at: public class Product
        :dedent: 4

Swashbuckle will generate the following request body:

    .. literalinclude:: ..\..\..\test\WebSites\FormMediaTypes\swagger.yaml
        :language: yaml
        :start-after: operationId: CreateProduct
        :end-before: responses:
        :dedent: 6