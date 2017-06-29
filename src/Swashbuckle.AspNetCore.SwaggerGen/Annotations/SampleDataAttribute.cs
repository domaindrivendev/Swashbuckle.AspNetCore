using System;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class SampleDataAttribute : Attribute
    {
        public SampleDataAttribute(object data)
        {
            this.Data = data;
        }

        public object Data { get; private set; }
    }
}
