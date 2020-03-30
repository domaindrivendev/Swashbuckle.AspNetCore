using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public static class ModelMetadataFactory
    {
        public static ModelMetadata CreateForType(Type type)
        {
            return new EmptyModelMetadataProvider().GetMetadataForType(type);
        }

        public static ModelMetadata CreateForProperty(Type containingType, string propertyName)
        {
            return new EmptyModelMetadataProvider().GetMetadataForProperty(containingType, propertyName);
        }

        public static ModelMetadata CreateForParameter(ParameterInfo parameter)
        {
            return new EmptyModelMetadataProvider().GetMetadataForParameter(parameter);
        }
    }
}