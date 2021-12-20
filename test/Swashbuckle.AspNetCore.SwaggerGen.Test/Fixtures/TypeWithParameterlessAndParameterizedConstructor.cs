namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TypeWithParameterlessAndParameterizedConstructor
    {
        public TypeWithParameterlessAndParameterizedConstructor()
        {}

        public TypeWithParameterlessAndParameterizedConstructor(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }
        public string Description { get; }
    }
}
