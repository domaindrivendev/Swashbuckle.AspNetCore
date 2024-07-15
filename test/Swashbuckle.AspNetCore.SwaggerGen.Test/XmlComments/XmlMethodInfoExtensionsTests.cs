using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class XmlMethodInfoExtensionsTests
    {
        [Theory, MemberData(nameof(DifferentMethodsSignatures))]
        public void DifferentMethodsSignatures_ShouldNotBeNull(MethodInfo methodInfo)
        {
            Assert.NotNull(methodInfo.GetUnderlyingGenericTypeMethod());
        }

        [Theory, MemberData(nameof(SimilarMethodsSignatures))]
        public void SimilarMethodsSignatures_ShouldBeNull(MethodInfo methodInfo)
        {
            Assert.Null(methodInfo.GetUnderlyingGenericTypeMethod());
        }

        public static TheoryData<MethodInfo> DifferentMethodsSignatures =>
            new(typeof(NonGenericResourceController).GetMethods()
                .Where(s =>
                {
                    return s.Name == nameof(NonGenericResourceController.DifferentMethodsSignatures);
                }));

        public static TheoryData<MethodInfo> SimilarMethodsSignatures =>
            new(typeof(NonGenericResourceController).GetMethods()
                .Where(s =>
                {
                    return s.Name == nameof(NonGenericResourceController.SimilarMethodsSignatures);
                }));
    }
}
