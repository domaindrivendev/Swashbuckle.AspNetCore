using Basic.Controllers;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Basic.Swagger
{
    public class AddCartDefault : IModelFilter
    {
        private ILogger<AddCartDefault> _logger;

        public AddCartDefault(ILogger<AddCartDefault> logger)
        {
            _logger = logger;
        }

        public void Apply(Schema model, ModelFilterContext context)
        {
            _logger.LogInformation("Applying an awesome IModelFilter that leverages Dependency Injection");

            model.Default = new
            {
                Id = 123
            };
        }
    }
}