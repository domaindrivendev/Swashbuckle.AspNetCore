using System;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class SampleDataSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (model?.Properties == null)
            {
                var type = context.SystemType;
                var attributes = type.GetTypeInfo().GetCustomAttributes<SampleDataAttribute>();
                var attribute = attributes?.FirstOrDefault();
                if (attribute != null)
                {
                    model.Example = attribute.Data;
                }

                return;
            }

            foreach (var property in model.Properties)
            {
                var typeProperty = GetClosestProperty(context.SystemType, property.Key);

                var attributes = typeProperty?.GetCustomAttributes<SampleDataAttribute>();
                var attribute = attributes?.FirstOrDefault();
                if (attribute != null)
                {
                    property.Value.Example = attribute.Data;
                }
            }
        }

        private static PropertyInfo GetClosestProperty(Type type, string name)
        {
            while (type != null)
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);

                if (property != null)
                {
                    return property;
                }

                type = type.GetTypeInfo().BaseType;
            }

            return null;
        }
    }
}