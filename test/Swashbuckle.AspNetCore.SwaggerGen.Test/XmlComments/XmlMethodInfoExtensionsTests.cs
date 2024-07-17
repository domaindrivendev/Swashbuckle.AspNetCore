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
    [ClassData(typeof(DifferentMethodsSignaturesData))]
    public void DifferentMethodsSignatures_ShouldBeExpected(MethodInfo methodInfo, IEnumerable<string> expectedParameterNames, Type expectedReturnType)
    {
        var underlyingGenericMethod = methodInfo.GetUnderlyingGenericTypeMethod();
        Assert.NotNull(underlyingGenericMethod);
        var underlyingGenericMethodParameters = underlyingGenericMethod.GetParameters();
        Assert.NotNull(underlyingGenericMethodParameters);
        Assert.NotEmpty(underlyingGenericMethodParameters);
        Assert.Equal(expectedParameterNames, underlyingGenericMethodParameters.Select(s => s.Name));
        Assert.NotNull(underlyingGenericMethod.ReturnType);
        Assert.Equal(expectedReturnType, underlyingGenericMethod.ReturnType);
    }

    public class DifferentMethodsSignaturesData : TheoryData<MethodInfo, IEnumerable<string>, Type>
    {
        public DifferentMethodsSignaturesData()
        {
            var methods = typeof(NonGenericResourceController).GetMethods()
            .Where(s => s.Name == nameof(NonGenericResourceController.DifferentMethodsSignatures));

            foreach (var method in methods)
            {
                Add(method, method.GetParameters().Select(p => p.Name), method.ReturnType);
            }
        }
    }

}
