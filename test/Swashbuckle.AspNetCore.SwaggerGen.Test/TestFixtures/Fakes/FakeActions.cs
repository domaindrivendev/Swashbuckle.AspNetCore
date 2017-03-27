using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class FakeActions
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

        public void AcceptsStringFromRoute([FromRoute]string param)
        { }

        public void AcceptsStringFromQuery([FromQuery]string param)
        { }

        public void AcceptsComplexTypeFromQuery([FromQuery]ComplexType param)
        { }

        public void AcceptsArrayFromQuery([FromQuery]IEnumerable<string> param)
        { }

        public void AcceptsArrayFromHeader([FromHeader]IEnumerable<string> param)
        { }

        public void AcceptsStringFromHeader([FromHeader]string param)
        { }

        public void AcceptsStringFromForm([FromForm]string param)
        { }

        public void AcceptsComplexTypeFromBody([FromBody]ComplexType param)
        { }

        public void AcceptsUnboundStringParameter(string param)
        { }

        public void AcceptsUnboundComplexParameter(ComplexType param)
        { }

        public void AcceptsCancellationToken(CancellationToken cancellationToken)
        { }

        [Obsolete]
        public void MarkedObsolete()
        { }

        public void AcceptsNestedType(ContainingType.NestedType param1)
        {}

        public void AcceptsGenericType(IEnumerable<string> param1)
        {}

        public void AcceptsGenericGenericType(IEnumerable<KeyValuePair<string, string>> param1)
        {}

        public void AcceptsGenericArrayType(KeyValuePair<string, string>[] param1)
        {}

        public void AcceptsXmlAnnotatedTypeFromQuery([FromQuery]XmlAnnotatedType param1)
        {}

        /// <summary>
        /// summary for AnnotatedWithXml
        /// </summary>
        /// <remarks>
        /// remarks for AnnotatedWithXml
        /// </remarks>
        /// <param name="param1">description for parma1</param>
        /// <param name="param2">description for param2</param>
        /// <response code="200">description for 200</param>
        /// <response code="400">description for 400</param>
        public void AnnotatedWithXml(int param1, IEnumerable<ComplexType> param2)
        { }

        /// <param name="param1">description for parma1</param>
        /// <param name="param2">description for param2</param>
        public void AnnotatedWithXmlHavingParameterNameBindings(
            [FromQuery(Name="p1")]string param1,
            [FromQuery(Name="p2")]string param2)
        { }

        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        public IActionResult AnnotatedWithResponseTypeAttributes()
        {
            throw new NotImplementedException();
        }

        [SwaggerOperation("CustomOperationId", Tags = new[] { "customTag" }, Schemes = new[] { "customScheme" })]
        public void AnnotatedWithSwaggerOperation()
        { }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void AnnotatedWithSwaggerOperationFilter()
        { }

        [SwaggerResponse(204, typeof(void), "No content is returned.")]
        [SwaggerResponse(400, typeof(IDictionary<string, string>), "This returns a dictionary.")]
        public IActionResult AnnotatedWithSwaggerResponseAttributes()
        {
            throw new NotImplementedException();
        }

        public class AnnotatedWithDataAnnotationsOperationRequest
        {
            [FromQuery(Name = "p1")]
            [Required]
            public string Param1 { get; set; }

            [FromQuery(Name = "p2")]
            public string Param2 { get; set; }

            [FromQuery(Name = "p3")]
            [RegularExpression("[0-9]*")]
            public string Param3 { get; set; }

            [FromQuery(Name = "p4")]
            [Range(100, 1000)]
            [DefaultValue(200)]
            public int Param4 { get; set; }

            [FromQuery(Name = "p5")]
            [MinLength(10)]
            public int Param5 { get; set; }

            [FromQuery(Name = "p6")]
            [MaxLength(100)]
            public int Param6 { get; set; }

            [FromQuery(Name = "p7")]
            [StringLength(50, MinimumLength = 25)]
            public int Param7 { get; set; }
        }

        public void AnnotatedWithDataAnnotationsOperation(
            AnnotatedWithDataAnnotationsOperationRequest request)
        { }
    }
}