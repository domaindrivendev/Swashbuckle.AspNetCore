using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class XmlMethodInfoExtensionsTests
{
    [Theory]
    [MemberData(nameof(DifferentMethodsSignatures))]
    public void DifferentMethodsSignatures_ShouldBeExpected((MethodInfo MethodInfo, IEnumerable<string> ExpectedParameterNames, Type ExpectedReturnParameterType) theoryData)
    {
        var underlyingGenericMethod = theoryData.MethodInfo.GetUnderlyingGenericTypeMethod();
        Assert.NotNull(underlyingGenericMethod);
        var underlyingGenericMethodParameters = underlyingGenericMethod.GetParameters();
        Assert.NotNull(underlyingGenericMethodParameters);
        Assert.NotEmpty(underlyingGenericMethodParameters);
        Assert.Equal(theoryData.ExpectedParameterNames, underlyingGenericMethodParameters.Select(s => s.Name));
        Assert.NotNull(underlyingGenericMethod.ReturnParameter);
        Assert.Equal(theoryData.ExpectedReturnParameterType, underlyingGenericMethod.ReturnParameter.ParameterType);
    }

    public static TheoryData<(MethodInfo MethodInfo, IEnumerable<string> ExpectedParameterNames, Type ExpectedReturnParameterType)> DifferentMethodsSignatures =>
        new(typeof(NonGenericResourceController).GetMethods()
            .Where(s => s.Name == nameof(NonGenericResourceController.DifferentMethodsSignatures))
            .Select(s => (s, s.GetParameters().Select(p => p.Name), s.ReturnParameter.ParameterType)));
}
