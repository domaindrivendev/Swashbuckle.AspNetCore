using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SchemaRegistryTests
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
        [InlineData(typeof(byte), "integer", "int32")]
        [InlineData(typeof(sbyte), "integer", "int32")]
        [InlineData(typeof(byte[]), "string", "byte")]
        [InlineData(typeof(bool), "boolean", null)]
        [InlineData(typeof(DateTime), "string", "date-time")]
        [InlineData(typeof(DateTimeOffset), "string", "date-time")]
        [InlineData(typeof(Guid), "string", "uuid")]
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
            Assert.Equal(new List<object> { AnEnum.Value1, AnEnum.Value2, AnEnum.X }, schema.Enum);
        }

        [Theory]
        [InlineData(typeof(int[]), "integer", "int32")]
        [InlineData(typeof(IEnumerable<string>), "string", null)]
        [InlineData(typeof(IEnumerable), "object", null)]
        public void GetOrRegister_ReturnsArraySchema_ForEnumerableTypes(
            Type systemType,
            string expectedItemsType,
            string expectedItemsFormat)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal("array", schema.Type);
            Assert.Equal(expectedItemsType, schema.Items.Type);
            Assert.Equal(expectedItemsFormat, schema.Items.Format);
        }

        [Fact]
        public void GetOrRegister_ReturnsMapSchema_ForDictionaryTypes()
        {
            var schema = Subject().GetOrRegister(typeof(Dictionary<string, string>));

            Assert.Equal("object", schema.Type);
            Assert.Equal("string", schema.AdditionalProperties.Type);
        }

        [Fact]
        public void GetOrRegister_ReturnsObjectSchema_ForDictionarTypesWithEnumKeys()
        {
            var schema = Subject().GetOrRegister(typeof(Dictionary<AnEnum, string>));

            Assert.Equal("object", schema.Type);
            Assert.NotNull(schema.Properties);
            Assert.Equal("string", schema.Properties["Value1"].Type);
            Assert.Equal("string", schema.Properties["Value2"].Type);
            Assert.Equal("string", schema.Properties["X"].Type);
        }

        [Theory]
        [InlineData(typeof(short?), "integer", "int32")]
        [InlineData(typeof(long?), "integer", "int64")]
        [InlineData(typeof(float?), "number", "float")]
        [InlineData(typeof(byte?), "integer", "int32")]
        [InlineData(typeof(DateTime?), "string", "date-time")]
        public void GetOrRegister_ReturnsPrimitiveSchema_ForNullableTypes(
            Type systemType,
            string expectedType,
            string expectedFormat)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal(expectedType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Theory]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<short>), "integer", "int32")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<long>), "integer", "int64")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<float>), "number", "float")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<byte>), "integer", "int32")]
        [InlineData(typeof(Microsoft.FSharp.Core.FSharpOption<DateTime>), "string", "date-time")]
        public void GetOrRegister_ReturnsPrimitiveSchema_ForOptionTypes(
            Type systemType,
            string expectedType,
            string expectedFormat)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal(expectedType, schema.Type);
            Assert.Equal(expectedFormat, schema.Format);
        }

        [Fact]
        public void GetOrRegister_ReturnsJsonReference_ForComplexTypes()
        {
            var reference = Subject().GetOrRegister(typeof(ComplexType));

            Assert.Equal("#/definitions/ComplexType", reference.Ref);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(JToken))]
        [InlineData(typeof(JToken))]
        public void GetOrRegister_ReturnsEmptyObjectSchema_ForAmbiguousTypes(Type systemType)
        {
            var schema = Subject().GetOrRegister(systemType);

            Assert.Equal("object", schema.Type);
            Assert.Null(schema.Properties);
        }

        [Fact]
        public void GetOrRegister_DefinesObjectSchema_ForComplexTypes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(ComplexType));

            var schema = subject.Definitions["ComplexType"];
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

        [Fact]
        public void GetOrRegister_DefinesObjectSchema_ForComplexTypesWithObjectExtensionData()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(ExtensionDataObjectType));

            var schema = subject.Definitions["ExtensionDataObjectType"];
            Assert.NotNull(schema);
            Assert.Equal("boolean", schema.Properties["Property1"].Type);
            Assert.Null(schema.Properties["Property1"].Format);
            Assert.NotNull(schema.AdditionalProperties);
            Assert.Equal("object", schema.AdditionalProperties.Type);
            Assert.Null(schema.AdditionalProperties.AdditionalProperties);
        }

        [Fact]
        public void GetOrRegister_IncludesInheritedProperties_ForSubTypes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(SubType));

            var schema = subject.Definitions["SubType"];
            Assert.Equal("string", schema.Properties["BaseProperty"].Type);
            Assert.Null(schema.Properties["BaseProperty"].Format);
            Assert.Equal("integer", schema.Properties["SubTypeProperty"].Type);
            Assert.Equal("int32", schema.Properties["SubTypeProperty"].Format);
        }

        [Fact]
        public void GetOrRegister_IncludesTypedProperties_ForDynamicObjectSubTypes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(DynamicObjectSubType));

            var schema = subject.Definitions["DynamicObjectSubType"];
            Assert.Equal(1, schema.Properties.Count);
            Assert.Equal("string", schema.Properties["Property1"].Type);
        }

        [Fact]
        public void GetOrRegister_IgnoresIndexerProperties_ForIndexedTypes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(IndexedType));

            var schema = subject.Definitions["IndexedType"];
            Assert.Equal(1, schema.Properties.Count);
            Assert.Contains("Property1", schema.Properties.Keys);
        }

        [Fact]
        public void GetOrRegister_HonorsJsonAttributes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(JsonAnnotatedType));

            var schema = subject.Definitions["JsonAnnotatedType"];
            Assert.Equal(2, schema.Properties.Count);
            Assert.Contains("foobar", schema.Properties.Keys);
            Assert.Equal(new[] { "Property3" }, schema.Required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsDataAttributes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(DataAnnotatedType));

            var schema = subject.Definitions["DataAnnotatedType"];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(5, schema.Properties["StringWithStringLength"].MinLength);
            Assert.Equal(10, schema.Properties["StringWithStringLength"].MaxLength);
            Assert.Equal(1, schema.Properties["StringWithMinMaxLength"].MinLength);
            Assert.Equal(3, schema.Properties["StringWithMinMaxLength"].MaxLength);
            Assert.Equal(new[] { "StringWithRequired", "NullableIntWithRequired" }, schema.Required.ToArray());
            Assert.Equal("date", schema.Properties["StringWithDataTypeDate"].Format);
            Assert.Equal("date-time", schema.Properties["StringWithDataTypeDateTime"].Format);
            Assert.Equal("password", schema.Properties["StringWithDataTypePassword"].Format);
        }

        [Fact]
        public void GetOrRegister_HonorsDataAttributes_ViaModelMetadataType()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(MetadataAnnotatedType));

            var schema = subject.Definitions["MetadataAnnotatedType"];
            Assert.Equal(1, schema.Properties["IntWithRange"].Minimum);
            Assert.Equal(12, schema.Properties["IntWithRange"].Maximum);
            Assert.Equal("^[3-6]?\\d{12,15}$", schema.Properties["StringWithRegularExpression"].Pattern);
            Assert.Equal(new[] { "StringWithRequired", "NullableIntWithRequired" }, schema.Required.ToArray());
        }

        [Fact]
        public void GetOrRegister_HonorsStringEnumConverters_ConfiguredViaAttributes()
        {
            var schema = Subject().GetOrRegister(typeof(JsonConvertedEnum));

            Assert.Equal("string", schema.Type);
            Assert.Equal(new List<object> { "Value1", "Value2", "X" }, schema.Enum);
        }

        [Fact]
        public void GetOrRegister_HonorsStringEnumConverters_ConfiguredViaSerializerSettings()
        {
            var subject = Subject(new JsonSerializerSettings
            {
                Converters = new[] { new StringEnumConverter { CamelCaseText = true } }
            });

            var schema = subject.GetOrRegister(typeof(AnEnum));

            Assert.Equal("string", schema.Type);
            Assert.Equal(new List<object> { "value1", "value2", "x" }, schema.Enum);
        }

        [Fact]
        public void GetOrRegister_HonorsEnumMemberAttributes()
        {
            var subject = Subject(new JsonSerializerSettings
            {
                Converters = new[] { new StringEnumConverter() }
            });

            var schema = subject.GetOrRegister(typeof(AnAnnotatedEnum));

            Assert.Equal("string", schema.Type);
            Assert.Equal(new[] { "foo-bar", "bar-foo", "Default" }, schema.Enum);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToExplicitlyMapTypes()
        {
            var subject = Subject(c =>
                c.CustomTypeMappings.Add(typeof(ComplexType), () => new Schema { Type = "string" })
            );

            var schema = subject.GetOrRegister(typeof(ComplexType));

            Assert.Equal("string", schema.Type);
            Assert.Null(schema.Properties);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(IDictionary<string, string>))]
        [InlineData(typeof(int[]))]
        [InlineData(typeof(ComplexType))]
        [InlineData(typeof(object))]
        public void GetOrRegister_SupportsOptionToPostModifyAllInlineSchemas(Type systemType)
        {
            var subject = Subject(c =>
                c.SchemaFilters.Add(new VendorExtensionsSchemaFilter())
            );

            var schemaOrRef = subject.GetOrRegister(systemType);

            var schema = (schemaOrRef.Ref == null)
                ? schemaOrRef
                : subject.Definitions[schemaOrRef.Ref.Replace("#/definitions/", "")];

            Assert.True(schema.Extensions.ContainsKey("X-property1"));
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToIgnoreObsoleteProperties()
        {
            var subject = Subject(c => c.IgnoreObsoleteProperties = true);

            subject.GetOrRegister(typeof(ObsoletePropertiesType));

            var schema = subject.Definitions["ObsoletePropertiesType"];
            Assert.DoesNotContain("ObsoleteProperty", schema.Properties.Keys);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToCustomizeSchemaIds()
        {
            var subject = Subject(c =>
            {
                c.SchemaIdSelector = (type) => type.FriendlyId(true).Replace("Swashbuckle.AspNetCore.SwaggerGen.Test.", "");
            });

            var jsonReference1 = subject.GetOrRegister(typeof(Namespace1.ConflictingType));
            var jsonReference2 = subject.GetOrRegister(typeof(Namespace2.ConflictingType));

            Assert.Equal("#/definitions/Namespace1.ConflictingType", jsonReference1.Ref);
            Assert.Equal("#/definitions/Namespace2.ConflictingType", jsonReference2.Ref);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToDescribeAllEnumsAsStrings()
        {
            var subject = Subject(c => c.DescribeAllEnumsAsStrings = true);

            var schema = subject.GetOrRegister(typeof(AnEnum));

            Assert.Equal("string", schema.Type);
            Assert.Equal(new List<object> { "Value1", "Value2", "X" }, schema.Enum);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToDescribeStringEnumsInCamelCase()
        {
            var subject = Subject(c =>
            {
                c.DescribeAllEnumsAsStrings = true;
                c.DescribeStringEnumsInCamelCase = true;
            });

            var schema = subject.GetOrRegister(typeof(AnEnum));

            Assert.Equal("string", schema.Type);
            Assert.Equal(new List<object> { "value1", "value2", "x" }, schema.Enum);
        }

        [Fact]
        public void GetOrRegister_SupportsOptionToUseReferencedDefinitionsForEnums()
        {
            var subject = Subject(c =>
            {
                c.UseReferencedDefinitionsForEnums = true;
            });

            var schema = subject.GetOrRegister(typeof(AnEnum));

            Assert.NotNull(schema.Ref);
            Assert.Contains(schema.Ref.Replace("#/definitions/", string.Empty), subject.Definitions.Keys);
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
            var subject = Subject();

            subject.GetOrRegister(typeof(CompositeType));

            var rootSchema = subject.Definitions["CompositeType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property1"].Ref);
            Assert.Equal("array", rootSchema.Properties["Property2"].Type);
            Assert.Equal("#/definitions/ComplexType", rootSchema.Properties["Property2"].Items.Ref);
            var componentSchema = subject.Definitions["ComplexType"];
            Assert.NotNull(componentSchema);
            Assert.Equal("object", componentSchema.Type);
            Assert.Equal(5, componentSchema.Properties.Count);
        }

        [Fact]
        public void GetOrRegister_HandlesNestedTypes()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(ContainingType));

            var rootSchema = subject.Definitions["ContainingType"];
            Assert.NotNull(rootSchema);
            Assert.Equal("object", rootSchema.Type);
            Assert.Equal("#/definitions/NestedType", rootSchema.Properties["Property1"].Ref);
            var nestedSchema = subject.Definitions["NestedType"];
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
            var subject = Subject();

            subject.GetOrRegister(systemType);

            Assert.Contains(expectedSchemaId, subject.Definitions.Keys);
        }

        [Fact]
        public void GetOrRegister_HandlesRecursion_IfCalledAgainWithinAFilter()
        {
            var subject = Subject(c => c.SchemaFilters.Add(new RecursiveCallSchemaFilter()));

            subject.GetOrRegister(typeof(object));
        }

        [Fact]
        public void GetOrRegister_Errors_OnConflictingClassName()
        {
            var subject = Subject();

            subject.GetOrRegister(typeof(Namespace1.ConflictingType));
            Assert.Throws<InvalidOperationException>(() =>
            {
                subject.GetOrRegister(typeof(Namespace2.ConflictingType));
            });
        }

        private SchemaRegistry Subject(Action<SchemaRegistrySettings> configure = null)
        {
            var settings = new SchemaRegistrySettings();
            if (configure != null) configure(settings);

            return new SchemaRegistry(new JsonSerializerSettings(), settings);
        }

        private SchemaRegistry Subject(JsonSerializerSettings jsonSerializerSettings)
        {
            return new SchemaRegistry(jsonSerializerSettings);
        }
    }
}