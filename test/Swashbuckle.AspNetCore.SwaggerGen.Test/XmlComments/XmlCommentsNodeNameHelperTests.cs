using System;
using Xunit;
using Xunit.Abstractions;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsNodeNameHelperTests
    {
        private readonly ITestOutputHelper _output;

        public XmlCommentsNodeNameHelperTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsNothing",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsNothing")]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsNestedType",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsNestedType(Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType)")]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsNestedThreeType",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsNestedThreeType(Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType.NestedTwoType.NestedThreeType)")]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsConstructedGenericType",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsConstructedGenericType(System.Collections.Generic.KeyValuePair{System.String,System.Int32})")]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsConstructedOfConstructedGenericType",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsConstructedOfConstructedGenericType(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.Int32}})")]
        [InlineData(typeof(XmlAnnotatedType), "AcceptsArrayOfConstructedGenericType",
            "M:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.AcceptsArrayOfConstructedGenericType(System.Nullable{System.Int32}[])")]
        public void GetMemberNameForMethod_ReturnsCorrectXmlCommentsMemberName_ForGivenMethodInfo(
            Type declaringType,
            string name,
            string expectedMemberName)
        {
            var methodInfo = declaringType.GetMethod(name);

            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForMethod(methodInfo);

            _output.WriteLine(expectedMemberName);
            _output.WriteLine(memberName);
            Assert.Equal(expectedMemberName, memberName);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType),
            "T:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType")]
        [InlineData(typeof(XmlAnnotatedType.NestedType),
            "T:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<int, string>),
            "T:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedGenericType`2")]
        [InlineData(typeof(NoNamespaceType), "T:NoNamespaceType")]
        [InlineData(typeof(NoNamespaceType.Nested1), "T:NoNamespaceType.Nested1")]
        [InlineData(typeof(NoNamespaceType.Nested1.Nested2), "T:NoNamespaceType.Nested1.Nested2")]
        [InlineData(typeof(NoNamespaceType.Nested1.Nested2.Nested3), "T:NoNamespaceType.Nested1.Nested2.Nested3")]
        [InlineData(typeof(XmlAnnotatedType.NestedType.NestedTwoType.NestedThreeType),
            "T:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType.NestedTwoType.NestedThreeType")]
        public void GetMemberNameForType_ReturnsCorrectXmlCommentsMemberName_ForGivenType(
            Type type,
            string expectedMemberName
        )
        {
            var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);

            _output.WriteLine(expectedMemberName);
            _output.WriteLine(memberName);
            Assert.Equal(expectedMemberName, memberName);
        }

        [Theory]
        [InlineData(typeof(XmlAnnotatedType), "StringProperty",
            "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.StringProperty")]
        [InlineData(typeof(XmlAnnotatedType), "StringField",
            "F:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.StringField")]
        [InlineData(typeof(XmlAnnotatedType.NestedType), "Property",
            "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType.Property")]
        [InlineData(typeof(XmlAnnotatedGenericType<int, string>), "GenericProperty",
            "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedGenericType`2.GenericProperty")]
        [InlineData(typeof(XmlAnnotatedType.NestedType.NestedTwoType), "Two",
            "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType.NestedTwoType.Two")]
        [InlineData(typeof(XmlAnnotatedType.NestedType.NestedTwoType.NestedThreeType), "Three",
            "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedType.NestedType.NestedTwoType.NestedThreeType.Three")]
        public void GetMemberNameForProperty_ReturnsCorrectXmlCommentMemberName_ForGivenMemberInfo(
            Type declaringType,
            string fieldOrPropertyName,
            string expectedMemberName
        )
        {
            var memberInfo = declaringType.GetMember(fieldOrPropertyName)[0];

            var memberName = XmlCommentsNodeNameHelper.GetNodeNameForMember(memberInfo);

            _output.WriteLine(expectedMemberName);
            _output.WriteLine(memberName);
            Assert.Equal(expectedMemberName, memberName);
        }
    }
}