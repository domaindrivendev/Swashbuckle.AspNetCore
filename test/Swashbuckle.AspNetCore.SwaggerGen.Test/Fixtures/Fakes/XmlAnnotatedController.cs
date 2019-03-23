using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// summary for XmlAnnotatedController
    /// </summary>
    /// <response code="400">controller-level description for 400</response>
    public class XmlAnnotatedController
    {
        /// <summary>
        /// summary for AnnotatedWithXml
        /// </summary>
        /// <remarks>
        /// remarks for AnnotatedWithXml
        /// </remarks>
        /// <param name="param1">description for param1</param>
        /// <param name="param2">description for param2</param>
        /// <param name="param3">description for param3</param>
        /// <response code="200">action-level description for 200</response>
        public void XmlAnnotatedAction(
            int param1,
            IEnumerable<ComplexType> param2,
            [FromQuery(Name = "Param-3")]string param3)
        { }

        public void AcceptsXmlAnnotatedTypeFromQuery([FromQuery]XmlAnnotatedType param1)
        {}
    }
}
