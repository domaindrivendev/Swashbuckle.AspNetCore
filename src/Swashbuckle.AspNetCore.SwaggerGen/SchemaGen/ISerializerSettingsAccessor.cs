using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISerializerSettingsAccessor
    {
        JsonSerializerSettings Value { get; }
    }
}
