using System;

namespace Swashbuckle.Swagger.Application
{
    public class SwashbuckleOptions
    {
        public SwashbuckleOptions()
        {
            SwaggerGeneratorOptions= new SwaggerGeneratorOptionsBuilder();
        }

        internal SwaggerGeneratorOptionsBuilder SwaggerGeneratorOptions { get; }

        public void SwaggerGenerator(Action<SwaggerGeneratorOptionsBuilder> configure)
        {
            configure(SwaggerGeneratorOptions);
        }
    }
}