namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISerializerDataContractResolver
{
    DataContract GetDataContractForType(Type type);
}
