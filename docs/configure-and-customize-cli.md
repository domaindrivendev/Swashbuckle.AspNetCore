# Configuration and Customization of `Swashbuckle.AspNetCore.Cli`

## Retrieve OpenAPI Directly from a Startup Assembly

Once your application has been set up with Swashbuckle.AspNetCore (see [Getting Started](../README.md#getting-started)), you can
use the Swashbuckle.OpenAPI CLI tool to retrieve OpenAPI JSON documents directly from your application's startup assembly, and write
it to a file. This can be useful if you want to incorporate OpenAPI generation into a CI/CD process, or if you want to serve
it from static file at run-time.

It's packaged as a [.NET Tool](https://learn.microsoft.com/dotnet/core/tools/global-tools) that can be installed and used via the .NET SDK.

> [!WARNING]
> The tool needs to load your Startup DLL and its dependencies at runtime. Therefore, you should use a version of the `dotnet` CLI
> that is compatible with your application. For example, if your app targets `net10.0`, then you should use version 10.0.xxx of the
> .NET SDK to run the CLI tool.

### Using the tool with the .NET SDK

#### Install as a Global Tool

To install as a [global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools#install-a-global-tool):

```terminal
dotnet tool install -g Swashbuckle.AspNetCore.Cli
```

#### Install as a local tool

1. In your solution root directory, create a tool manifest file:

    ```terminal
    dotnet new tool-manifest
    ```

2. Install as a [local tool](https://learn.microsoft.com/dotnet/core/tools/global-tools#install-a-local-tool)

    ```terminal
    dotnet tool install Swashbuckle.AspNetCore.Cli
    ```

### Usage

1. Verify that the tool was installed correctly

    ```termainal
    swagger tofile --help
    ```

2. Generate an OpenAPI document from your application's startup assembly

    ```terminal
    swagger tofile --output [output] [startupassembly] [swaggerdoc]
    ```

    Placeholders and their meaning:
    * `[output]`: the relative path where the OpenAPI JSON document will be output to;
    * `[startupassembly]`: the relative path to your application's startup assembly;
    * `[swaggerdoc]`: the name of the OpenAPI document you want to generate, as configured in your application.

## Use the CLI Tool with a Custom Host Configuration

By default, the tool will execute in the context of a "default" web host. However, in some cases you may want to
bring your own host environment, for example if you've configured a custom DI container such as Autofac. For this
scenario, the Swashbuckle CLI tool exposes a convention-based hook for your application.

That is, if your application contains a class that meets either of the following naming conventions, then that class
will be used to provide a host for the CLI tool to run in.

* `public class SwaggerHostFactory`, containing a public static method called `CreateHost` with return type `IHost`
* `public class SwaggerWebHostFactory`, containing a public static method called `CreateWebHost` with return type `IWebHost`

For example, the following class could be used to leverage the same host configuration as your application:

```csharp
public class SwaggerHostFactory
{
    public static IHost CreateHost()
        => Program.CreateHostBuilder([]).Build();
}
```
