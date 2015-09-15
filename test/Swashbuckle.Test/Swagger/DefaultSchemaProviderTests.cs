using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Xunit;
using Swashbuckle.Fixtures;
using Swashbuckle.Fixtures.Extensions;

namespace Swashbuckle.Swagger
{
    public class DefaultSchemaProviderTests
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
        public void GetSchema_ReturnsPrimitiveSchema_ForSimpleTypes(
            Type systemType,
            string expectedType,
            string expectedFormat)
        {
            var schema = Subject().GetSchema(systemType, new Dictionary<string, Schema>());

            Assert.Equal(expectedType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Fact]
        public void GetSchema_ReturnsEnumSchema_ForEnumTypes()
        {
            var schema = Subject().GetSchema(typeof(AnEnum), new Dictionary<string, Schema>());
            Assert.Equal("integer", schema.Type);
            Assert.Equal("int32", schema.Format);
            Assert.Contains(AnEnum.Value1, schema.Enum);
            Assert.Contains(AnEnum.Value2, schema.Enum);
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32", null)]
        [InlineData(typeof(IEnumerable<string>), "string", null, null)]
        [InlineData(typeof(IEnumerable), null, null, "#/definitions/Object")]
        public void GetSchema_ReturnsArraySchema_ForEnumerableTypes(
            Type systemType,
            string expectedItemsType,
            string expectedItemsFormat,
            string expectedItemsRef)
        {
            var schema = Subject().GetSchema(systemType, new Dictionary<string, Schema>());

            Assert.Equal("array", schema.Type);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
            Assert.Equal(expectedItemsRef, schema.Items.Ref);
        }

        [Fact]
        public void GetSchema_ReturnsMapSchema_ForDictionaryTypes()
        {
            var schema = Subject().GetSchema(
                typeof(Dictionary<string, string>),
                new Dictionary<string, Schema>());

            Assert.Equal("object", schema.Type);
            Assert.Equal("string", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GetSchema_ReturnsJsonReference_ForComplexTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            var reference = Subject().GetSchema(typeof(ComplexType), definitions);

            Assert.Equal("#/definitions/ComplexType", reference.Ref);
        }

        [Fact]
        public void GetSchema_DefinesObjectSchema_ForComplexTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(ComplexType), definitions);

            var schema = definitions["ComplexType"];
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
        public void GetSchema_DefinesBaseObjectSchema_ForRuntimeTypes(Type systemType)
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(systemType, definitions);

            var schema = definitions["Object"];
            Assert.NotNull(schema);
            Assert.Empty(schema.Properties);
        }

        [Fact]
        public void GetSchema_IncludesInheritedProperties_ForSubTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(SubType), definitions);

            var schema = definitions["SubType"];
            Assert.Equal("string", schema.Properties["BaseProperty"].Type);
            Assert.Null(schema.Properties["BaseProperty"].Format);
            Assert.Equal("integer", schema.Properties["SubTypeProperty"].Type);
            Assert.Equal("int32", schema.Properties["SubTypeProperty"].Format);
        }

        [Fact]
        public void GetSchema_IgnoresIndexerProperties_ForIndexedTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(IndexedType), definitions);

            var schema = definitions["IndexedType"];
            Assert.Equal(1, schema.Properties.Count);
            Assert.Contains("Property1", schema.Properties.Keys);
        }

        [Fact]
        public void GetSchema_HonorsJsonAttributes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(JsonAnnotatedType), definitions);

            var schema = definitions["JsonAnnotatedType"];
            Assert.Equal(2, schema.Properties.Count);
            Assert.Contains("foobar", schema.Properties.Keys);
            Assert.Equal(new[] { "Property3" }, schema.Required.ToArray());
        }

        [Fact]
        public void GetSchema_HonorsDataAttributes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(DataAnnotatedType), definitions);

            var schema = definitions["DataAnnotatedType"];
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
        public void GetSchema_HonorsStringEnumConverters_ConfiguredViaAttributes()
        {
            var schema = Subject().GetSchema(typeof(JsonConvertedEnum), new Dictionary<string, Schema>());

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "Value1", "Value2", "X" }, schema.Enum);
        }

        [Fact]
        public void GetSchema_HonorsStringEnumConverters_ConfiguredViaSerializerSettings()
        {
            var subject = Subject(new JsonSerializerSettings
            {
                Converters = new[] { new StringEnumConverter { CamelCaseText = true } }
            });

            var schema = subject.GetSchema(typeof(AnEnum), new Dictionary<string, Schema>());

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "value1", "value2", "x" }, schema.Enum);
        }

        [Fact]
        public void GetSchema_SupportsOptionToExplicitlyMapTypes()
        {
            var subject = Subject(opts =>
                opts.MapType<ComplexType>(() => new Schema { Type = "string" })
            );

            var schema = subject.GetSchema(typeof(ComplexType), new Dictionary<string, Schema>());

            Assert.Equal("string", schema.Type);
            Assert.Null(schema.Properties);
        }

        [Fact]
        public void GetSchema_SupportsOptionToPostModifyObjectSchemas()
        {
            var definitions = new Dictionary<string, Schema>();
            var subject = Subject(opts =>
                opts.ModelFilter<VendorExtensionsModelFilter>()
            );

            subject.GetSchema(typeof(ComplexType), definitions);

            var schema = definitions["ComplexType"];
            Assert.NotEmpty(schema.Extensions);
        }

        [Fact]
        public void GetSchema_SupportsOptionToIgnoreObsoleteProperties()
        {
            var definitions = new Dictionary<string, Schema>();
            var subject = Subject(opts => opts.IgnoreObsoleteProperties = true);

            subject.GetSchema(typeof(ObsoletePropertiesType), definitions);

            var schema = definitions["ObsoletePropertiesType"];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GetSchema_SupportsOptionToCustomizeSchemaIds()
        {
            var subject = Subject(opts =>
            {
                opts.CustomSchemaIds((type) => type.FriendlyId(true).Replace("Swashbuckle.Fixtures.", ""));
            });

            var definitions = new Dictionary<string, Schema>();
            var jsonReference1 = subject.GetSchema(typeof(Fixtures.Namespace1.ConflictingType), definitions);
            var jsonReference2 = subject.GetSchema(typeof(Fixtures.Namespace2.ConflictingType), definitions);

            Assert.Equal("#/definitions/Namespace1.ConflictingType", jsonReference1.Ref);
            Assert.Equal("#/definitions/Namespace2.ConflictingType", jsonReference2.Ref);
        }

        [Fact]
        public void GetSchema_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var subject = Subject(opts => opts.DescribeAllEnumsAsStrings = true);

            var schema = subject.GetSchema(typeof(AnEnum), new Dictionary<string, Schema>());

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "Value1", "Value2", "X" }, schema.Enum);
        }

        [Fact]
        public void GetSchema_SupportsOptionToDescribeStringEnumsInCamelCase()
        {
            var subject = Subject(opts =>
            {
                opts.DescribeAllEnumsAsStrings = true;
                opts.DescribeStringEnumsInCamelCase = true;
            });

            var schema = subject.GetSchema(typeof(AnEnum), new Dictionary<string, Schema>());

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "value1", "value2", "x" }, schema.Enum);
        }

        [Fact]
        public void GetSchema_HandlesMultiDemensionalArrays()
        {
            var schema = Subject().GetSchema(typeof(int[][]), new Dictionary<string, Schema>());

            Assert.Equal("array", schema.Type);
            Assert.Equal("array", schema.Items.Type);
            Assert.Equal("integer", schema.Items.Items.Type);
            Assert.Equal("int32", schema.Items.Items.Format);
        }

        [Fact]
        public void GetSchema_HandlesCompositeTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(CompositeType), definitions);

            var rootSchema = definitions["CompositeType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property1"].Ref);
            Assert.Equal("array", rootSchema.Properties["Property2"].Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property2"].Items.Ref);
            var componentSchema = definitions["ComplexType"];
            Assert.NotNull(componentSchema);
            Assert.Equal("object", componentSchema.Type);
            Assert.Equal(5, componentSchema.Properties.Count);
        }

        [Fact]
        public void GetSchema_HandlesNestedTypes()
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(typeof(ContainingType), definitions);

            var rootSchema = definitions["ContainingType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/NestedType", rootSchema.Properties["Property1"].Ref);
            var nestedSchema = definitions["NestedType"];
            Assert.NotNull(nestedSchema);
            Assert.Equal("object", nestedSchema.Type);
            Assert.Equal(1, nestedSchema.Properties.Count);
        }

        [Theory]
        [InlineData(typeof(SelfReferencingType), "SelfReferencingType")]
        [InlineData(typeof(ListOfSelf), "ListOfSelf")]
        [InlineData(typeof(DictionaryOfSelf), "DictionaryOfSelf")]
        public void GetSchema_HandlesSelfReferencingTypes(
            Type systemType,
            string expectedSchemaId)
        {
            var definitions = new Dictionary<string, Schema>();

            Subject().GetSchema(systemType, definitions);

            Assert.Contains(expectedSchemaId, definitions.Keys);
        }

        private DefaultSchemaProvider Subject(Action<SwaggerSchemaOptions> configureOptions = null)
        {
            var options = new SwaggerSchemaOptions();
            if (configureOptions != null) configureOptions(options);

            return new DefaultSchemaProvider(new JsonSerializerSettings(), options);
        }

        private DefaultSchemaProvider Subject(JsonSerializerSettings jsonSerializerSettings)
        {
            return new DefaultSchemaProvider(jsonSerializerSettings);
        }
    }
}