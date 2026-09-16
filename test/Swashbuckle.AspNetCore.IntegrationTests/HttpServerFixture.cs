using Microsoft.AspNetCore.Mvc.Testing;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class HttpApplicationFixture<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    public HttpApplicationFixture() => UseKestrel(0);

    public string ServerUrl
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }
}
