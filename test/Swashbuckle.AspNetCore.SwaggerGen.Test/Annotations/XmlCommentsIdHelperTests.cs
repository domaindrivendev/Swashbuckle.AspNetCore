using System;
using System.Reflection;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlCommentsIdHelperTests
    {
        [Theory]
        [InlineData(nameof(FakeActions.AcceptsNothing), "M:Swashbuckle.AspNetCore.SwaggerGen.Test.FakeActions.AcceptsNothing")]
        [InlineData(nameof(FakeActions.AcceptsNestedType), "M:Swashbuckle.AspNetCore.SwaggerGen.Test.FakeActions.AcceptsNestedType(Swashbuckle.AspNetCore.SwaggerGen.Test.ContainingType.NestedType)")]
        [InlineData(nameof(FakeActions.AcceptsGenericType), "M:Swashbuckle.AspNetCore.SwaggerGen.Test.FakeActions.AcceptsGenericType(System.Collections.Generic.IEnumerable{System.String})")]
        [InlineData(nameof(FakeActions.AcceptsGenericGenericType), "M:Swashbuckle.AspNetCore.SwaggerGen.Test.FakeActions.AcceptsGenericGenericType(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})")]
        [InlineData(nameof(FakeActions.AcceptsGenericArrayType), "M:Swashbuckle.AspNetCore.SwaggerGen.Test.FakeActions.AcceptsGenericArrayType(System.Collections.Generic.KeyValuePair{System.String,System.String}[])")]
        public void GetCommentIdForMethod_ReturnsCorrectXmlCommentId_ForGivenMethodInfo(
            string actionFixtureName,
            string expectedCommentId
        )
        {
            var methodInfo = typeof(FakeActions).GetMethod(actionFixtureName);

            var commentId = XmlCommentsIdHelper.GetCommentIdForMethod(methodInfo);

            Assert.Equal(expectedCommentId, commentId);
        }

        [Theory]
        [InlineData(typeof(ContainingType.NestedType), "T:Swashbuckle.AspNetCore.SwaggerGen.Test.ContainingType.NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<>), "T:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedGenericType`1")]
        [InlineData(typeof(NoNamespaceType), "T:NoNamespaceType")]
        public void GetCommentIdForType_ReturnsCorrectXmlCommentId_ForGivenType(
            Type type,
            string expectedCommentId
        )
        {
            var commentId = XmlCommentsIdHelper.GetCommentIdForType(type);

            Assert.Equal(expectedCommentId, commentId);
        }

        [Theory]
        [InlineData(typeof(ContainingType.NestedType), nameof(ContainingType.NestedType.Property2), "P:Swashbuckle.AspNetCore.SwaggerGen.Test.ContainingType.NestedType.Property2")]
        [InlineData(typeof(XmlAnnotatedGenericType<>), "GenericProperty", "P:Swashbuckle.AspNetCore.SwaggerGen.Test.XmlAnnotatedGenericType`1.GenericProperty")]
        public void GetCommentIdForProperty_ReturnsCorrectXmlCommentId_ForGivenPropertyInfo(
            Type type,
            string propertyName,
            string expectedCommentId
        )
        {
            var propertyInfo = type.GetProperty(propertyName);

            var commentId = XmlCommentsIdHelper.GetCommentIdForMember(propertyInfo);

            Assert.Equal(expectedCommentId, commentId);
        }
    }
}
