using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class MetadataAttributesSchemaFilterTests
    {

        [Fact]
        public void Apply_ModelMetadataSchemaFilter_ViaDataAnnotatedType()
        {
            var subject = Subject();
            var context = FilterContextFor(typeof(DataAnnotatedType));
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
                {
                    { "StringWithNoAttributes", TypeMap[typeof(string)]() },
                    { "IntWithRange", TypeMap[typeof(int)]() },
                    { "StringWithRegularExpression", TypeMap[typeof(string)]() },
                    { "StringWithStringLength", TypeMap[typeof(string)]() },
                    { "StringWithMinMaxLength", TypeMap[typeof(string)]() },
                    { "StringWithRequired", TypeMap[typeof(string)]() },
                    { "IntWithRequired", TypeMap[typeof(int)]() },
                    { "StringWithDataTypeDate", TypeMap[typeof(string)]() },
                    { "StringWithDataTypeDateTime", TypeMap[typeof(string)]() },
                    { "StringWithDataTypePassword", TypeMap[typeof(string)]() },
                    { "StringWithDefaultValue", TypeMap[typeof(string)]() },
                    { "StringWithDescription", TypeMap[typeof(string)]() },
                    { "StringWithDisplayName", TypeMap[typeof(string)]() }
                }
            };
            subject.Apply(schema, context);
            
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
            Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
            Assert.Equal(1, schema.Properties["StringWithMinMaxLength"].MinLength);
            Assert.Equal(3, schema.Properties["StringWithMinMaxLength"].MaxLength);
            Assert.Equal(new[] { "StringWithRequired", "IntWithRequired" }, schema.Required.ToArray());
            Assert.Equal("date", schema.Properties["StringWithDataTypeDate"].Format);
            Assert.Equal("date-time", schema.Properties["StringWithDataTypeDateTime"].Format);
            Assert.Equal("password", schema.Properties["StringWithDataTypePassword"].Format);
            Assert.Equal("foobar", schema.Properties["StringWithDefaultValue"].Default);
            Assert.Equal("Description", schema.Properties["StringWithDescription"].Description);
            Assert.Equal("DisplayName", schema.Properties["StringWithDisplayName"].Title);

        }

        [Fact]
        public void Apply_ModelMetadataSchemaFilter_ViaModelMetadataType()
        {
            var subject = Subject();
            var context = FilterContextFor(typeof(MetadataAnnotatedType));
            var schema = new Schema
            {
                Properties = new Dictionary<string, Schema>()
                {
                    { "IntWithRange", TypeMap[typeof(int)]() },
                    { "StringWithRegularExpression", TypeMap[typeof(string)]() },
                    { "StringWithRequired", TypeMap[typeof(string)]() },
                    { "IntWithRequired", TypeMap[typeof(int)]() }
                }
            };
            subject.Apply(schema, context);
            
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(new[] { "StringWithRequired", "IntWithRequired" }, schema.Required.ToArray());
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            var jsonObjectContract = new DefaultContractResolver().ResolveContract(type);
            var metaData = new EmptyModelMetadataProvider().GetMetadataForType(type);
            return new SchemaFilterContext(type, metaData, (jsonObjectContract as JsonObjectContract), null);
        }

        private ModelMetaDataSchemaFilter Subject()
        {
            return new ModelMetaDataSchemaFilter();
        }
        private static readonly Dictionary<Type, Func<Schema>> TypeMap = new Dictionary<Type, Func<Schema>>
        {
            { typeof(short), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new Schema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new Schema { Type = "number", Format = "float" } },
            { typeof(double), () => new Schema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new Schema { Type = "number", Format = "double" } },
            { typeof(byte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new Schema { Type = "integer", Format = "int32" } },
            { typeof(byte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(sbyte[]), () => new Schema { Type = "string", Format = "byte" } },
            { typeof(bool), () => new Schema { Type = "boolean" } },
            { typeof(DateTime), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new Schema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new Schema { Type = "string", Format = "uuid" } },
            { typeof(string), () => new Schema { Type = "string" } },
            { typeof(IEnumerable<>), () => new Schema { Type = "array" } }
        };
    }
}
