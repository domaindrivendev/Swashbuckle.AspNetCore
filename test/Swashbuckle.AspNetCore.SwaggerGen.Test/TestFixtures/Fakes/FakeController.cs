using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeController
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

        public JObject ReturnsJObject()
        {
            throw new NotImplementedException();
        }

        public IActionResult ReturnsActionResult()
        {
            throw new NotImplementedException();
        }

        public void AcceptsNothing()
        { }

        public void AcceptsString(string param)
        { }

        public void AcceptsComplexType(ComplexType param)
        { }

        public void AcceptsBindingAnnotatedParams(string stringWithNoAttributes)
        { }

        // Use this version when https://github.com/aspnet/Mvc/issues/7435 is resolved
        //public void AcceptsBindingAnnotatedParams(
        //    string stringWithNoAttributes,
        //    [BindRequired]string stringWithBindRequired)
        //    [BindRequired]int intWithBindRequired)
        //    [BindNever]int intWithBindNever)
        //{ }

        public void AcceptsDataAnnotatedParams(
            string stringWithNoAttributes,
            [Required]string stringWithRequired,
            [Required]int intWithRequired)
        { }

        public void AcceptsBindingAnnotatedType(BindingAnnotatedType param)
        { }

        public void AcceptsDataAnnotatedType(DataAnnotatedType param)
        { }

        public void AcceptsOptionalParameter(string param = "foobar")
        { }

        public void AcceptsStringFromRoute([FromRoute]string param)
        { }

        public void AcceptsStringFromQuery([FromQuery]string param)
        { }

        public void AcceptsIntegerFromQuery([FromQuery]int param)
        { }

        public void AcceptsArrayFromQuery([FromQuery]IEnumerable<string> param)
        { }

        public void AcceptsStringFromHeader([FromHeader]string param)
        { }

        public void AcceptsStringFromForm([FromForm]string param)
        { }

        public void AcceptsComplexTypeFromBody([FromBody]ComplexType param)
        { }

        public void AcceptsIFormFile(IFormFile formFile)
        { }

        public void AcceptsCancellationToken(CancellationToken cancellationToken)
        { }

        public void AcceptsXmlAnnotatedTypeFromQuery([FromQuery]XmlAnnotatedType param1)
        {}

        /// <summary>
        /// summary for AnnotatedWithXml
        /// </summary>
        /// <remarks>
        /// remarks for AnnotatedWithXml
        /// </remarks>
        /// <param name="param1">description for param1</param>
        /// <param name="param2">description for param2</param>
        /// <param name="param3">description for param3</param>
        /// <response code="200">description for 200</response>
        /// <response code="400">description for 400</response>
        public void AnnotatedWithXml(
            int param1,
            IEnumerable<ComplexType> param2,
            [FromQuery(Name = "Param-3")]string param3)
        { }

        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        public IActionResult AnnotatedWithResponseTypeAttributes()
        {
            throw new NotImplementedException();
        }

        [Consumes("application/xml")]
        public IActionResult AnnotatedWithConsumes([FromBody]ComplexType param1)
        {
            throw new NotImplementedException();
        }

        [Produces("application/xml")]
        public IActionResult AnnotatedWithProduces()
        {
            throw new NotImplementedException();
        }

        [Obsolete]
        public void MarkedObsolete()
        { }
    }
}