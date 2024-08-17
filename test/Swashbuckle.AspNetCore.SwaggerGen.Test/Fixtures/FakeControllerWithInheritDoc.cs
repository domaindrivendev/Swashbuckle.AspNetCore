using System;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

/// <inheritdoc cref = "FakeControllerWithXmlComments" />
public class FakeControllerWithInheritDoc : FakeControllerWithXmlComments
{
    /// <inheritdoc cref = "FakeControllerWithXmlComments.ActionWithSummaryAndRemarksTags"/>
    public new void ActionWithSummaryAndRemarksTags()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref = "FakeControllerWithXmlComments.ActionWithParamTags"/>
    public new void ActionWithParamTags(string param1, string param2)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref = "FakeControllerWithXmlComments.ActionWithResponseTags"/>
    public new void ActionWithResponseTags()
    {
        throw new NotImplementedException();
    }
}
