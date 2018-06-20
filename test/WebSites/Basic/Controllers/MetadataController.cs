using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Controllers
{
    [Route("/metadata")]
    [Produces("application/json")]
    public class MetadataController
    {
        [SwaggerResponse(StatusCodes.Status200OK, typeof(MetaDataResponse))]
        [HttpPost]
        public IActionResult Create([FromBody, Required]ModelMetadataRequest body)
        {
            return new JsonResult(body)
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        
    }
    /// <summary>
    /// Summary
    /// </summary>
    [DisplayName("Metadata Request")]
    public class ModelMetadataRequest
    {
        [DisplayName("Child Object Reference")]
        public Child ChildObject { get; set; }
        [DisplayName("String")]
        public string String { get; set; }
        [DisplayName("List of Strings")]
        public List<string> Array { get; set; }
    }
    [DisplayName("Child Object")]
    [Description("Recursive Reference Description")]
    public class Child
    {
        [Description("Recursive Reference")]
        [DisplayName("Recursive List")]
        public List<Child> RecrusiveRef { get; set; }
        [DisplayName("Item")]
        public string Item { get; set; }
    }
    [Description("Metadata Response object description")]
    [DisplayName("Metadata Response")]
    public class MetaDataResponse : ModelMetadataRequest
    {
        [Description("Response Int")]
        public int Int { get; set; }
    }


}