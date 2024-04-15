using System.Text.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TypeWithParameterlessAndJsonAnnotatedConstructor
    {
        public TypeWithParameterlessAndJsonAnnotatedConstructor()
        {}

        [JsonConstructor]
        public TypeWithParameterlessAndJsonAnnotatedConstructor(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }
        public string Description { get; }
    }
}
