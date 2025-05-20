# Configuration & Customization of `Swashbuckle.AspNetCore.Cli`

## Retrieve Swagger Directly from a Startup Assembly 

Once your application has been setup with Swashbuckle (see [Getting Started](../README.md#getting-started)), you can use the Swashbuckle CLI tool to retrieve Swagger / OpenAPI JSON directly from your application's startup assembly, and write it to file. This can be useful if you want to incorporate Swagger generation into a CI/CD process, or if you want to serve it from static file at run-time.

It's packaged as a [.NET Tool](https://learn.microsoft.com/dotnet/core/tools/global-tools) that can be installed and used via the dotnet SDK.

> [!WARNING] 
> The tool needs to load your Startup DLL and its dependencies at runtime. Therefore, you should use a version of the `dotnet` SDK that is compatible with your application. For example, if your app targets `net8.0`, then you should use version 8.0.xxx of the SDK to run the CLI tool. If it targets `net9.0`, then you should use version 9.0.xxx of the SDK and so on.

### Using the tool with the .NET SDK

1. Install as a [global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools#install-a-global-tool)

    ```
    dotnet tool install -g Swashbuckle.AspNetCore.Cli
    ```

2. Verify that the tool was installed correctly

    ```
    swagger tofile --help
    ```

3. Generate a Swagger/ OpenAPI document from your application's startup assembly

    ```
    swagger tofile --output [output] [startupassembly] [swaggerdoc]
    ```

    Placeholders and their meaning:
    * `[output]`: the relative path where the Swagger JSON will be output to
    * `[startupassembly]`: the relative path to your application's startup assembly
    * `[swaggerdoc]`: the name of the swagger document you want to retrieve, as configured in your startup class

### Using the tool with the .NET 6.0 SDK or later

1. In your project root, create a tool manifest file:

    ```
    dotnet new tool-manifest
    ```

2. Install as a [local tool](https://learn.microsoft.com/dotnet/core/tools/global-tools#install-a-local-tool)

    ```
    dotnet tool install Swashbuckle.AspNetCore.Cli
    ```

3. Verify that the tool was installed correctly

    ```
    dotnet swagger tofile --help
    ```

4. Generate a Swagger / OpenAPI document from your application's startup assembly

    ```
    dotnet swagger tofile --output [output] [startupassembly] [swaggerdoc]
    ```

    Placeholders and their meaning:
    * `[output]`: the relative path where the Swagger JSON will be output to
    * `[startupassembly]`: the relative path to your application's startup assembly
    * `[swaggerdoc]`: the name of the swagger document you want to retrieve, as configured in your startup class

## Use the CLI Tool with a Custom Host Configuration

Out-of-the-box, the tool will execute in the context of a "default" web host. However, in some cases you may want to bring your own host environment, for example if you've configured a custom DI container such as Autofac. For this scenario, the Swashbuckle CLI tool exposes a convention-based hook for your application.

That is, if your application contains a class that meets either of the following naming conventions, then that class will be used to provide a host for the CLI tool to run in.

- `public class SwaggerHostFactory`, containing a public static method called `CreateHost` with return type `IHost`
- `public class SwaggerWebHostFactory`, containing a public static method called `CreateWebHost` with return type `IWebHost`

For example, the following class could be used to leverage the same host configuration as your application:

```csharp
public class SwaggerHostFactory
{
    public static IHost CreateHost()
    {
        return Program.CreateHostBuilder(new string[0]).Build();
    }
}
```
