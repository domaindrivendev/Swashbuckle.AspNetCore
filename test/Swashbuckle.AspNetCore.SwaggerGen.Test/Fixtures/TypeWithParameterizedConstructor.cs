namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TypeWithParameterizedConstructor
    {
        public TypeWithParameterizedConstructor(int id, string desc)
        {
            Id = id;
            Description = desc;
        }

        public int Id { get; }
        public string Description { get; }
    }
}
