using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using Xunit;
using Swashbuckle.Fixtures;
using Swashbuckle.Fixtures.Extensions;
using Swashbuckle.Application;

namespace Swashbuckle.Swagger
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

            Assert.Equal(expectedType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Fact]
        public void GetOrRegister_ReturnsEnumSchema_ForEnumTypes()
        {
            var schema = Subject().GetOrRegister(typeof(AnEnum));
            Assert.Equal("integer", schema.Type);
            Assert.Equal("int32", schema.Format);
            Assert.Contains(AnEnum.Value1, schema.Enum);
            Assert.Contains(AnEnum.Value2, schema.Enum);
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

            Assert.Equal("array", schema.Type);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
            Assert.Equal(expectedItemsRef, schema.Items.Ref);
        }

        [Fact]
        public void GetOrRegister_ReturnsMapSchema_ForDictionaryTypes()
        {
            var schema = Subject().GetOrRegister(typeof(Dictionary<string, string>));

            Assert.Equal("object", schema.Type);
            Assert.Equal("string", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GetOrRegister_ReturnsJsonReference_ForComplexTypes()
        {
            var jsonReference = Subject().GetOrRegister(typeof(ComplexType));

            Assert.Equal("#/definitions/ComplexType", jsonReference.Ref);
        }

        [Fact]
        public void GetOrRegister_RegistersObjectSchema_ForComplexTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(ComplexType));

            var schema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotNull(schema);
            Assert.Equal("boolean", schema.Properties["Property1"].Type);
            Assert.Null(schema.Properties["Property1"].Format);
            Assert.Equal("string", schema.Properties["Property2"].Type);
            Assert.Equal("date-time", schema.Properties["Property2"].Format);
            Assert.Equal("string", schema.Properties["Property3"].Type);
            Assert.Equal("date-time", schema.Properties["Property3"].Format);
            Assert.Equal("string", schema.Properties["Property4"].Type);
            Assert.Null(schema.Properties["Property4"].Format);
            Assert.Equal("string", schema.Properties["Property5"].Type);
            Assert.Null(schema.Properties["Property5"].Format);
        }

        [Theory]
        [InlineData(typeof(JObject))]
        [InlineData(typeof(JToken))]
        public void GetOrRegister_RegistersBaseObjectSchema_ForRuntimeTypes(Type systemType)
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(systemType);

            var schema = schemaGenerator.Definitions["Object"];
            Assert.NotNull(schema);
            Assert.Empty(schema.Properties);
        }

        [Fact]
        public void GetOrRegister_IncludesInheritedProperties_ForSubTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(SubType));

            var schema = schemaGenerator.Definitions["SubType"];
            Assert.Equal("string", schema.Properties["BaseProperty"].Type);
            Assert.Null(schema.Properties["BaseProperty"].Format);
            Assert.Equal("integer", schema.Properties["SubTypeProperty"].Type);
            Assert.Equal("int32", schema.Properties["SubTypeProperty"].Format);
        }

        [Fact]
        public void GetOrRegister_IgnoresIndexerProperties_ForIndexedTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(IndexedType));

            var schema = schemaGenerator.Definitions["IndexedType"];
            Assert.Equal(1, schema.Properties.Count);
            Assert.Contains("Property1", schema.Properties.Keys);
        }

        [Fact]
        public void GetOrRegister_HonorsJsonAttributes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(JsonAnnotatedType));

            var schema = schemaGenerator.Definitions["JsonAnnotatedType"];
            Assert.Equal(2, schema.Properties.Count);
            Assert.Contains("foobar", schema.Properties.Keys);
            Assert.Equal(new[] { "Property3" }, schema.Required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsDataAttributes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(DataAnnotatedType));

            var schema = schemaGenerator.Definitions["DataAnnotatedType"];
            Assert.Equal(1, schema.Properties["RangeProperty"].Minimum);
            Assert.Equal(12, schema.Properties["RangeProperty"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["PatternProperty"].Pattern);
            Assert.Equal(5, schema.Properties["StringProperty1"].MinLength);
            Assert.Equal(10, schema.Properties["StringProperty1"].MaxLength);
            Assert.Equal(1, schema.Properties["StringProperty2"].MinLength);
            Assert.Equal(3, schema.Properties["StringProperty2"].MaxLength);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["PatternProperty"].Pattern);
            Assert.Equal(new[] { "RangeProperty", "PatternProperty" }, schema.Required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsJsonConvertedEnums()
        {
            var schema = Subject().GetOrRegister(typeof(JsonConvertedEnum));
            
            Assert.Equal("string", schema.Type);
            Assert.Contains("Value1", schema.Enum);
            Assert.Contains("Value2", schema.Enum);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToExplicitlyMapTypes()
        {
            var schemaGenerator = Subject(opts =>
                opts.MapType<ComplexType>(() => new Schema { Type = "string" })
            );

            var schema = schemaGenerator.GetOrRegister(typeof(ComplexType));

            Assert.Equal("string", schema.Type);
            Assert.Null(schema.Properties);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToPostModifyObjectSchemas()
        {
            var schemaGenerator = Subject(opts =>
                opts.ModelFilter<VendorExtensionsModelFilter>()
            );

            schemaGenerator.GetOrRegister(typeof(ComplexType));

            var schema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotEmpty(schema.Extensions);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToIgnoreObsoleteProperties()
        {
            var schemaGenerator = Subject(opts => opts.IgnoreObsoleteProperties = true);

            schemaGenerator.GetOrRegister(typeof(ObsoletePropertiesType));

            var schema = schemaGenerator.Definitions["ObsoletePropertiesType"];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToUseFullTypeNamesInSchemaIds()
        {
            var schemaGenerator = Subject(opts => opts.UseFullTypeNameInSchemaIds = true);

            var jsonReference1 = schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace1.ConflictingType));
            var jsonReference2 = schemaGenerator.GetOrRegister(typeof(Fixtures.Namespace2.ConflictingType));

            Assert.Equal("#/definitions/Swashbuckle.Fixtures.Namespace1.ConflictingType", jsonReference1.Ref);
            Assert.Equal("#/definitions/Swashbuckle.Fixtures.Namespace2.ConflictingType", jsonReference2.Ref);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var schemaGenerator = Subject(opts => opts.DescribeAllEnumsAsStrings = true);

            var schema = schemaGenerator.GetOrRegister(typeof(AnEnum));

            Assert.Equal("string", schema.Type);
            Assert.Contains("Value1", schema.Enum);
            Assert.Contains("Value2", schema.Enum);
        }

        [Fact]
        public void GetOrRegister_HandlesMultiDemensionalArrays()
        {
            var schema = Subject().GetOrRegister(typeof(int[][]));

            Assert.Equal("array", schema.Type);
            Assert.Equal("array", schema.Items.Type);
            Assert.Equal("integer", schema.Items.Items.Type);
            Assert.Equal("int32", schema.Items.Items.Format);
        }

        [Fact]
        public void GetOrRegister_HandlesCompositeTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(CompositeType));

            var rootSchema = schemaGenerator.Definitions["CompositeType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property1"].Ref);
            Assert.Equal("array", rootSchema.Properties["Property2"].Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property2"].Items.Ref);
            var componentSchema = schemaGenerator.Definitions["ComplexType"];
            Assert.NotNull(componentSchema);
            Assert.Equal("object", componentSchema.Type);
            Assert.Equal(5, componentSchema.Properties.Count);
        }

        [Fact]
        public void GetOrRegister_HandlesNestedTypes()
        {
            var schemaGenerator = Subject();

            schemaGenerator.GetOrRegister(typeof(ContainingType));

            var rootSchema = schemaGenerator.Definitions["ContainingType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/NestedType", rootSchema.Properties["Property1"].Ref);
            var nestedSchema = schemaGenerator.Definitions["NestedType"];
            Assert.NotNull(nestedSchema);
            Assert.Equal("object", nestedSchema.Type);
            Assert.Equal(1, nestedSchema.Properties.Count);
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

        private SchemaGenerator Subject(Action<SchemaGeneratorOptions> configure = null)
        {
            var options = new SchemaGeneratorOptions();
            if (configure != null) configure(options);

            return new SchemaGenerator(new DefaultContractResolver(), options);
        }
    }
}