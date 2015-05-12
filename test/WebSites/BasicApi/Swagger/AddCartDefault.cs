using Swashbuckle.Swagger;

namespace BasicApi.Swagger
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
