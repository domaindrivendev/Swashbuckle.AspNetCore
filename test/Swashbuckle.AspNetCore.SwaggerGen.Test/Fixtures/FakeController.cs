using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class FakeController
{
    public void ActionWithNoParameters()
    { }

    public void ActionWithParameter(string param)
    { }

    public void ActionWithMultipleParameters(string param1, int param2)
    { }

    [HttpGet(Name = "SomeRouteName")]
    public void ActionWithRouteNameMetadata()
    { }

    [Obsolete]
    public void ActionWithObsoleteAttribute()
    { }

    public void ActionWithParameterWithBindNeverAttribute([BindNever] string param)
    { }

    public void ActionWithParameterWithRequiredAttribute([Required] string param)
    { }

    public void ActionWithParameterWithBindRequiredAttribute([BindRequired] string param)
    { }

    public void ActionWithIntParameter(int param)
    { }

    public void ActionWithIntParameterWithRangeAttribute([Range(1, 12)] int param)
    { }

    public void ActionWithIntParameterWithDefaultValue(int param = 1)
    { }

    public void ActionWithIntParameterWithDefaultValueAttribute([DefaultValue(3)] int param)
    { }

    public void ActionWithIntFromQueryParameter([FromQuery] int param)
    { }

    public void ActionWithIntParameterWithRequiredAttribute([Required] int param)
    { }

    public void ActionWithIntParameterWithSwaggerIgnoreAttribute([SwaggerIgnore] int param)
    { }

    public void ActionWithAcceptFromHeaderParameter([FromHeader] string accept, string param)
    { }

    public void ActionWithContentTypeFromHeaderParameter([FromHeader(Name = "Content-Type")] string contentType, string param)
    { }

    public void ActionWithAuthorizationFromHeaderParameter([FromHeader] string authorization, string param)
    { }

    public void ActionWithObjectParameter(XmlAnnotatedType param)
    { }

    public void ActionWithDerivedObjectParameter(AbcTests_C param)
    { }

    // Helper test types for GenerateSchema_PreservesIntermediateBaseProperties_WhenUsingOneOfPolymorphism
    public abstract class AbcTests_A
    {
        public string PropA { get; set; }
    }

    public class AbcTests_B : AbcTests_A
    {
        public string PropB { get; set; }
    }

    public class AbcTests_C : AbcTests_B
    {
        public string PropC { get; set; }
    }

    public class TypeWithRequiredProperty
    {
        public required string RequiredProperty { get; set; }
    }

    public void ActionWithRequiredMember(TypeWithRequiredProperty param)
    { }

    [Consumes("application/someMediaType")]
    public void ActionWithConsumesAttribute(string param)
    { }

    public int ActionWithReturnValue()
    {
        throw new NotImplementedException();
    }

    [Produces("application/someMediaType")]
    public int ActionWithProducesAttribute()
    {
        throw new NotImplementedException();
    }

    [ProducesResponseType(typeof(FileContentResult), 200, "application/zip")]
    public FileContentResult ActionWithFileResult()
    {
        throw new NotImplementedException();
    }

    [SwaggerIgnore]
    public void ActionWithSwaggerIgnoreAttribute()
    { }

    public void ActionHavingIFormFileParamWithFromFormAttribute([FromForm] IFormFile fileUpload)
    { }

    public void ActionHavingFromFormAttributeButNotWithIFormFile([FromForm] string param1, IFormFile param2)
    { }
    public void ActionHavingFromFormAttributeWithSwaggerIgnore([FromForm] SwaggerIngoreAnnotatedType param1)
    { }
    public void ActionHavingEnum(IntEnum param1)
    { }
    public void ActionHavingFromFormObjectAndString([FromForm] SwaggerIngoreAnnotatedType param1, [FromForm] string param2)
    { }
}
