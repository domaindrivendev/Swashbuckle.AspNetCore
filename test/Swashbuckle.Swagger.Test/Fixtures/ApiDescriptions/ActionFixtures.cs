using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Fixtures.ApiDescriptions
{
    public class ActionFixtures
    {
        public void ReturnsVoid()
        { }

        public int ReturnsInt()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> ReturnsEnumerable()
        {
            throw new NotImplementedException();
        }

        public ComplexType ReturnsComplexType()
        {
            throw new NotImplementedException();
        }

        public IActionResult ReturnsActionResult()
        {
            throw new NotImplementedException();
        }

        public void AcceptsNothing()
        { }

        public void AcceptsStringFromRoute([FromRoute]string param)
        { }

        public void AcceptsStringFromQuery([FromQuery]string param)
        { }

        public void AcceptsComplexTypeFromQuery([FromQuery]ComplexType param)
        { }

        public void AcceptsRequiredStringFromQuery([FromQuery][Required]string param)
        { }

        public void AcceptsStringFromHeader([FromHeader]string param)
        { }

        public void AcceptsStringFromBody([FromBody]ComplexType param)
        { }

        public void AcceptsComplexTypeFromBody([FromBody]ComplexType param)
        { }

        [Obsolete]
        public void MarkedObsolete()
        { }
    }
}