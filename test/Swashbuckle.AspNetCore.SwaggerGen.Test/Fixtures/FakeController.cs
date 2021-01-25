using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
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

        public void ActionWithParameterWithBindNeverAttribute([BindNever]string param)
        { }

        public void ActionWithParameterWithRequiredAttribute([Required]string param)
        { }

        public void ActionWithParameterWithBindRequiredAttribute([BindRequired]string param)
        { }

        public void ActionWithIntParameter(int param)
        { }

        public void ActionWithIntParameterWithRangeAttribute([Range(1, 12)]int param)
        { }

        public void ActionWithIntParameterWithDefaultValue(int param = 1)
        { }

        public void ActionWithIntParameterWithDefaultValueAttribute([DefaultValue(3)]int param)
        { }

        public void ActionWithIntParameterWithRequiredAttribute([Required]int param)
        { }

        public void ActionWithObjectParameter(XmlAnnotatedType param)
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
    }
}