using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Swashbuckle.SwaggerGen.Annotations;
using Swashbuckle.SwaggerGen.TestFixtures.Extensions;

namespace Swashbuckle.SwaggerGen.TestFixtures
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

        public void AcceptsStringFromHeader([FromHeader]string param)
        { }

        public void AcceptsComplexTypeFromBody([FromBody]ComplexType param)
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

        /// <summary>
        /// summary for AnnotatedWithXml
        /// </summary>
        /// <remarks>
        /// remarks for AnnotatedWithXml
        /// </remarks>
        /// <param name="param1">description for parma1</param>
        /// <param name="param2">description for param2</param>
        public void AnnotatedWithXml(int param1, IEnumerable<ComplexType> param2)
        { }

        [SwaggerOperation("CustomOperationId", Tags = new[] { "customTag" }, Schemes = new[] { "customScheme" })]
        public void AnnotatedWithSwaggerOperation()
        { }

        [SwaggerResponseRemoveDefaults]
        public void AnnotatedWithSwaggerResponseRemoveDefaults()
        {}

        [SwaggerResponse(HttpStatusCode.OK, "Action defined 200", typeof(int))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Action defined 400", typeof(int))]
        public int AnnotatedWithSwaggerResponses(ComplexType param)
        {
            throw new NotImplementedException();
        }

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public void AnnotatedWithSwaggerOperationFilter()
        { }
   }
}