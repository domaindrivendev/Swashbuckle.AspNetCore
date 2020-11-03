using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IDataContractResolver
    {
        bool CanResolveContractFor(Type type);

        DataContract ResolveContractFor(Type type);
    }
}