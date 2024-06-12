using System;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Basic.Controllers
{
    [Route("Enum")]
    public class EnumController
    {
        [HttpGet("DisplaySimpleEnum", Name = nameof(GetDateTimeKind))]
        [SwaggerOperation("Display Enum description")]
        public DateTimeKind GetDateTimeKind([FromQuery] DateTimeKind dateTimeKind)
        {
            return dateTimeKind;
        }

        [HttpGet("DisplayArrayEnum", Name = nameof(GetDateTimeKindArray))]
        [SwaggerOperation("Display Enum description")]
        public DateTimeKind[] GetDateTimeKindArray([FromQuery] DateTimeKind[] dateTimeKind)
        {
            return dateTimeKind;
        }
    }
}
