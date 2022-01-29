namespace Swashbuckle.AspNetCore.SwaggerGen.SchemaValidator
{
    public interface ISchemaValidator
    {
        SchemaValidationResult ValidateControllerResponses();
    }
}