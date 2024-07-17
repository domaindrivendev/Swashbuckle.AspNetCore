using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlMethodInfoExtensionsTests
{
    [Theory]
    [MemberData(nameof(DifferentMethodsSignatures))]
    public void DifferentMethodsSignatures_ShouldBeExpected(MethodInfo methodInfo)
    {
        var underlyingGenericMethod = methodInfo.GetUnderlyingGenericTypeMethod();
        Assert.NotNull(underlyingGenericMethod);
        var underlyingGenericMethodParameters = underlyingGenericMethod.GetParameters();
        Assert.NotNull(underlyingGenericMethodParameters);
        Assert.NotEmpty(underlyingGenericMethodParameters);
        Assert.Equal(methodInfo.GetParameters().Select(s => s.Name), underlyingGenericMethodParameters.Select(s => s.Name));
        Assert.NotNull(underlyingGenericMethod.ReturnParameter);
        Assert.Equal(methodInfo.ReturnParameter.ParameterType, underlyingGenericMethod.ReturnParameter.ParameterType);
    }

    public static TheoryData<MethodInfo> DifferentMethodsSignatures =>
        new(typeof(NonGenericResourceController).GetMethods()
            .Where(s =>
            {
                return s.Name == nameof(NonGenericResourceController.DifferentMethodsSignatures);
            }));
}
