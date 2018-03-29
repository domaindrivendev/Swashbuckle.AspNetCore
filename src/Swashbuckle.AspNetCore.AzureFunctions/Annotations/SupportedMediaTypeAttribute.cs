using System;

namespace Swashbuckle.AspNetCore.AzureFunctions.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SupportedMediaTypeAttribute : Attribute
    {
        public string MediaType { get; }

        public SupportedMediaTypeAttribute(string mediaType)
        {
            MediaType = mediaType;
        }
    }
}