using System;

namespace Swashbuckle.AspNetCore.AzureFunctions.Annotations
{
    /// <summary>
    /// Use this attribute to ignore a function or function parameter for swagger generation
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }
}