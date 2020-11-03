using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class FileDataContractResolver : IDataContractResolver
    {
        public bool CanResolveContractFor(Type type)
        {
            return type.IsAssignableToOneOf(typeof(IFormFile), typeof(FileResult));
        }

        public DataContract ResolveContractFor(Type type)
        {
            return DataContract.Primitive(type, DataType.String, "binary");
        }
    }
}
