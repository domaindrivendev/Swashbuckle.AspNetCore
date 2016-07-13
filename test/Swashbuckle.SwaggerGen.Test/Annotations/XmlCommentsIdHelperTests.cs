using System;
using System.Reflection;
using Xunit;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class XmlCommentsIdHelperTests
    {
        [Theory]
        [InlineData(nameof(FakeActions.AcceptsNothing), "M:Swashbuckle.SwaggerGen.TestFixtures.FakeActions.AcceptsNothing")]
        [InlineData(nameof(FakeActions.AcceptsNestedType), "M:Swashbuckle.SwaggerGen.TestFixtures.FakeActions.AcceptsNestedType(Swashbuckle.SwaggerGen.TestFixtures.ContainingType.NestedType)")]
        [InlineData(nameof(FakeActions.AcceptsGenericType), "M:Swashbuckle.SwaggerGen.TestFixtures.FakeActions.AcceptsGenericType(System.Collections.Generic.IEnumerable{System.String})")]
        [InlineData(nameof(FakeActions.AcceptsGenericGenericType), "M:Swashbuckle.SwaggerGen.TestFixtures.FakeActions.AcceptsGenericGenericType(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})")]
        [InlineData(nameof(FakeActions.AcceptsGenericArrayType), "M:Swashbuckle.SwaggerGen.TestFixtures.FakeActions.AcceptsGenericArrayType(System.Collections.Generic.KeyValuePair{System.String,System.String}[])")]
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
        [InlineData(typeof(ContainingType.NestedType), "T:Swashbuckle.SwaggerGen.TestFixtures.ContainingType.NestedType")]
        [InlineData(typeof(XmlAnnotatedGenericType<>), "T:Swashbuckle.SwaggerGen.TestFixtures.XmlAnnotatedGenericType`1")]
        public void GetCommentIdForType_ReturnsCorrectXmlCommentId_ForGivenType(
            Type type,
            string expectedCommentId
        )
        {
            var commentId = XmlCommentsIdHelper.GetCommentIdForType(type);

            Assert.Equal(expectedCommentId, commentId);
        }

        [Theory]
        [InlineData(typeof(ContainingType.NestedType), nameof(ContainingType.NestedType.Property2), "P:Swashbuckle.SwaggerGen.TestFixtures.ContainingType.NestedType.Property2")]
        [InlineData(typeof(XmlAnnotatedGenericType<>), "GenericProperty", "P:Swashbuckle.SwaggerGen.TestFixtures.XmlAnnotatedGenericType`1.GenericProperty")]
        public void GetCommentIdForProperty_ReturnsCorrectXmlCommentId_ForGivenPropertyInfo(
            Type type,
            string propertyName,
            string expectedCommentId
        )
        {
            var propertyInfo = type.GetProperty(propertyName);

            var commentId = XmlCommentsIdHelper.GetCommentIdForProperty(propertyInfo);

            Assert.Equal(expectedCommentId, commentId);
        }
    }
}
