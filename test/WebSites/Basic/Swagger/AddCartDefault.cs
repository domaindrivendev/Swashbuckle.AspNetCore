using Swashbuckle.SwaggerGen.Generator;

namespace Basic.Swagger
{
    public class AddCartDefault : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.Default = new
            {
                Id = "myCartId"
            };
        }
    }
}
