using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.OpenAPI.Application
{
    public class ConversionException : Exception
    {
        public ConversionException(string errors)
            : base(string.Format("Error converting Swagger to OpenAPI", errors))
        { }
    }
}
