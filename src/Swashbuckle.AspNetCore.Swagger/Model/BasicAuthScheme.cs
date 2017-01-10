namespace Swashbuckle.AspNetCore.Swagger
{
    public class BasicAuthScheme : SecurityScheme
    {
        public BasicAuthScheme()
        {
            Type = "basic";
        }
    }
}
