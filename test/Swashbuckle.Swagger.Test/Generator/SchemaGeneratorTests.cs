using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;
using Swashbuckle.Swagger.Generator;
using Swashbuckle.Swagger.Application;
using Swashbuckle.Swagger.Fixtures;
using Swashbuckle.Swagger.Fixtures.Extensions;
using Swashbuckle.Swagger.Application;

namespace Swashbuckle.Swagger.Generator
{
    public class SchemaGeneratorTests
    {
        [Theory]
        [InlineData(typeof(short), "integer", "int32")]
        [InlineData(typeof(ushort), "integer", "int32")]
        [InlineData(typeof(int), "integer", "int32")]
        [InlineData(typeof(uint), "integer", "int32")]
        [InlineData(typeof(long), "integer", "int64")]
        [InlineData(typeof(ulong), "integer", "int64")]
        [InlineData(typeof(float), "number", "float")]
        [InlineData(typeof(double), "number", "double")]
        [InlineData(typeof(decimal), "number", "double")]
        [InlineData(typeof(byte), "string", "byte")]
        [InlineData(typeof(sbyte), "string", "byte")]
        [InlineData(typeof(bool), "boolean", null)]
        [InlineData(typeof(DateTime), "string", "date-time")]
        [InlineData(typeof(DateTimeOffset), "string", "date-time")]
        [InlineData(typeof(string), "string", null)]
        public void GetOrRegister_ReturnsPrimitiveSchema_ForSimpleTypes(
            Type systemType,
            string expectedType,
            string expectedFormat)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal(expectedType, schema.type);
            Assert.Equal(expectedFormat, schema.format);
        }

        [Fact]
        public void GetOrRegister_ReturnsEnumSchema_ForEnumTypes()
        {
            var schema = Subject().GetOrRegister(typeof(AnEnum));
            Assert.Equal("integer", schema.type);
            Assert.Equal("int32", schema.format);
            Assert.Contains(AnEnum.Value1, schema.@enum);
            Assert.Contains(AnEnum.Value2, schema.@enum);
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32", null)]
        [InlineData(typeof(IEnumerable<string>), "string", null, null)]
        [InlineData(typeof(IEnumerable), null, null, "#/definitions/Object")]
        public void GetOrRegister_ReturnsArraySchema_ForEnumerableTypes(
            Type systemType,
            string expectedItemsType,
            string expectedItemsFormat,
            string expectedItemsRef)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal("array", schema.type);
            Assert.Equal(expectedItemsType, schema.items.type);
            Assert.Equal(expectedItemsFormat, schema.items.format);
            Assert.Equal(expectedItemsRef, schema.items.@ref);
        }

        [Fact]
        public void GetOrRegister_ReturnsMapSchema_ForDictionaryTypes()
        {
            var schema = Subject().GetOrRegister(typeof(Dictionary<string, string>));

            Assert.Equal("object", schema.type);
            Assert.Equal("string", schema.additionalProperties.type);
        }

        [Fact]
        public void GetOrRegister_ReturnsJsonReference_ForComplexTypes()
        {
            var jsonReference = Subject().GetOrRegister(typeof(ComplexType));

            Assert.Equal("#/definitions/ComplexType", jsonReference.@ref);
        }

        [Fact]
        public void GetOrRegister_RegistersObjectSchema_ForComplexTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(ComplexType));

            var schema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotNull(schema);
            Assert.Equal("boolean", schema.properties["Property1"].type);
            Assert.Null(schema.properties["Property1"].format);
            Assert.Equal("string", schema.properties["Property2"].type);
            Assert.Equal("date-time", schema.properties["Property2"].format);
            Assert.Equal("string", schema.properties["Property3"].type);
            Assert.Equal("date-time", schema.properties["Property3"].format);
            Assert.Equal("string", schema.properties["Property4"].type);
            Assert.Null(schema.properties["Property4"].format);
            Assert.Equal("string", schema.properties["Property5"].type);
            Assert.Null(schema.properties["Property5"].format);
        }

        [Theory]
        [InlineData(typeof(JObject))]
        [InlineData(typeof(JToken))]
        [InlineData(typeof(IActionResult))]
        public void GetOrRegister_RegistersBaseObjectSchema_ForRuntimeTypes(Type systemType)
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(systemType);

            var schema = schemaGenerator.Definitions["Object"];
            Assert.NotNull(schema);
            Assert.Empty(schema.properties);
        }

        [Fact]
        public void GetOrRegister_IncludesInheritedProperties_ForSubTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(SubType));

            var schema = schemaGenerator.Definitions["SubType"];
            Assert.Equal("string", schema.properties["BaseProperty"].type);
            Assert.Null(schema.properties["BaseProperty"].format);
            Assert.Equal("integer", schema.properties["SubTypeProperty"].type);
            Assert.Equal("int32", schema.properties["SubTypeProperty"].format);
        }

        [Fact]
        public void GetOrRegister_IgnoresIndexerProperties_ForIndexedTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(IndexedType));

            var schema = schemaGenerator.Definitions["IndexedType"];
            Assert.Equal(1, schema.properties.Count);
            Assert.Contains("Property1", schema.properties.Keys);
        }

        [Fact]
        public void GetOrRegister_HonorsJsonAttributes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(JsonAnnotatedType));

            var schema = schemaGenerator.Definitions["JsonAnnotatedType"];
            Assert.Equal(2, schema.properties.Count);
            Assert.Contains("foobar", schema.properties.Keys);
            Assert.Equal(new[] { "Property3" }, schema.required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsDataAttributes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(DataAnnotatedType));

            var schema = schemaGenerator.Definitions["DataAnnotatedType"];
            Assert.Equal(1, schema.properties["RangeProperty"].minimum);
            Assert.Equal(12, schema.properties["RangeProperty"].maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.properties["PatternProperty"].pattern);
            Assert.Equal(5, schema.properties["StringProperty1"].minLength);
            Assert.Equal(10, schema.properties["StringProperty1"].maxLength);
            Assert.Equal(1, schema.properties["StringProperty2"].minLength);
            Assert.Equal(3, schema.properties["StringProperty2"].maxLength);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.properties["PatternProperty"].pattern);
            Assert.Equal(new[] { "RangeProperty", "PatternProperty" }, schema.required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsJsonConvertedEnums()
        {
            var schema = Subject().GetOrRegister(typeof(JsonConvertedEnum));
            
            Assert.Equal("string", schema.type);
            Assert.Contains("Value1", schema.@enum);
            Assert.Contains("Value2", schema.@enum);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToExplicitlyMapTypes()
        {
            var schemaGenerator = Subject(opts =>
                opts.MapType<ComplexType>(() => new Schema { type = "string" })
            );

            var schema = schemaGenerator.GetOrRegister(typeof(ComplexType));

            Assert.Equal("string", schema.type);
            Assert.Null(schema.properties);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToPostModifyObjectSchemas()
        {
            var schemaGenerator = Subject(opts =>
                opts.ModelFilter<VendorExtensionsSchemaFilter>()
            );

            schemaGenerator.GetOrRegister(typeof(ComplexType));

            var schema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotEmpty(schema.vendorExtensions);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToIgnoreObsoleteProperties()
        {
            var schemaGenerator = Subject(opts => opts.IgnoreObsoleteProperties());

            schemaGenerator.GetOrRegister(typeof(ObsoletePropertiesType));

            var schema = schemaGenerator.Definitions["ObsoletePropertiesType"];
            Assert.DoesNotContain("ObsoleteProperty", schema.properties.Keys);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToUseFullTypeNamesInSchemaIds()
        {
            var schemaGenerator = Subject(opts => opts.UseFullTypeNameInSchemaIds());

            var jsonReference1 = schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace1.ConflictingType));
            var jsonReference2 = schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace2.ConflictingType));

            Assert.Equal("#/definitions/Swashbuckle.Swagger.Fixtures.Namespace1.ConflictingType", jsonReference1.@ref);
            Assert.Equal("#/definitions/Swashbuckle.Swagger.Fixtures.Namespace2.ConflictingType", jsonReference2.@ref);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var schemaGenerator = Subject(opts => opts.DescribeAllEnumsAsStrings());

            var schema = schemaGenerator.GetOrRegister(typeof(AnEnum));

            Assert.Equal("string", schema.type);
            Assert.Contains("Value1", schema.@enum);
            Assert.Contains("Value2", schema.@enum);
        }

        [Fact]
        public void GetOrRegister_HandlesMultiDemensionalArrays()
        {
            var schema = Subject().GetOrRegister(typeof(int[][]));

            Assert.Equal("array", schema.type);
            Assert.Equal("array", schema.items.type);
            Assert.Equal("integer", schema.items.items.type);
            Assert.Equal("int32", schema.items.items.format);
        }

        [Fact]
        public void GetOrRegister_HandlesNestedComplexTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(NestedComplexType));

            var rootSchema = schemaGenerator.Definitions["NestedComplexType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.properties["Property1"].@ref);
            Assert.Equal("array", rootSchema.properties["Property2"].type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.properties["Property2"].items.@ref);
            var nestedSchema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotNull(nestedSchema);
            Assert.Equal("object", nestedSchema.type);
            Assert.Equal(5, nestedSchema.properties.Count);
        }

        [Theory]
        [InlineData(typeof(SelfReferencingType), "SelfReferencingType")]
        [InlineData(typeof(ListOfSelf), "ListOfSelf")]
        [InlineData(typeof(DictionaryOfSelf), "DictionaryOfSelf")]
        public void GetOrRegister_HandlesSelfReferencingTypes(
            Type systemType,
            string expectedSchemaId)
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(systemType);

            Assert.Contains(expectedSchemaId, schemaGenerator.Definitions.Keys);
        }

        [Fact]
        public void GetOrRegister_Errors_OnConflictingClassName()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace1.ConflictingType));
            Assert.Throws<InvalidOperationException>(() =>
            {
                schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace2.ConflictingType));
            });
        }

        private SchemaGenerator Subject(Action<SchemaGeneratorOptionsBuilder> setupOptions = null)
        {
            var optionsBuilder = new SchemaGeneratorOptionsBuilder();
            if (setupOptions != null) setupOptions(optionsBuilder);

            return new SchemaGenerator(new DefaultContractResolver(), optionsBuilder.Build());
        }
    }
}