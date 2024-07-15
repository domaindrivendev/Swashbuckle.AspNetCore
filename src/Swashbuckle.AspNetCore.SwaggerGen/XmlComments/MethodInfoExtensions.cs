using System.Linq;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class MethodInfoExtensions
    {
        public static MethodInfo GetUnderlyingGenericTypeMethod(this MethodInfo constructedTypeMethod)
        {
            var constructedType = constructedTypeMethod.DeclaringType;
            var genericTypeDefinition = constructedType.GetGenericTypeDefinition();
            var genericArguments = constructedType.GenericTypeArguments;

            var constructedTypeParameters = constructedTypeMethod.GetParameters();

            // Retrieve list of candidate methods that match name and parameter count
            var candidateMethods = genericTypeDefinition.GetMethods()
                .Where(m =>
                {
                    var genericTypeDefinitionParameters = m.GetParameters();
                    if (m.Name == constructedTypeMethod.Name && genericTypeDefinitionParameters.Length == constructedTypeParameters.Length)
                    {
                        for (var i = 0; i < genericTypeDefinitionParameters.Length; i++)
                        {
                            if (genericTypeDefinitionParameters[i].ParameterType.IsArray && constructedTypeParameters[i].ParameterType.IsArray)
                            {
                                var genericTypeDefinitionElement = genericTypeDefinitionParameters[i].ParameterType.GetElementType();
                                var constructedTypeDefinitionElement = constructedTypeParameters[i].ParameterType.GetElementType();
                                if (genericTypeDefinitionElement.IsGenericParameter && genericArguments.Any(p => p == constructedTypeDefinitionElement))
                                {
                                    continue;
                                }
                                else if (genericTypeDefinitionElement != constructedTypeDefinitionElement)
                                {
                                    return false;
                                }
                            }
                            else if (genericTypeDefinitionParameters[i].ParameterType.IsConstructedGenericType && constructedTypeParameters[i].ParameterType.IsConstructedGenericType)
                            {
                                if (genericTypeDefinitionParameters[i].ParameterType.GetGenericTypeDefinition() != constructedTypeParameters[i].ParameterType.GetGenericTypeDefinition())
                                {
                                    return false;
                                }
                                var genericTypeDefinitionArguments = genericTypeDefinitionParameters[i].ParameterType.GetGenericArguments();
                                var constructedDefinitionArguments = constructedTypeParameters[i].ParameterType.GetGenericArguments();
                                if (genericTypeDefinitionArguments.Length != constructedDefinitionArguments.Length)
                                {
                                    return false;
                                }
                                for (var j = 0; j < genericTypeDefinitionArguments.Length; j++)
                                {
                                    if (genericTypeDefinitionArguments[j].IsGenericParameter && genericArguments.Any(p => p == constructedDefinitionArguments[j]))
                                    {
                                        continue;
                                    }
                                    else if (genericTypeDefinitionArguments[j] != constructedDefinitionArguments[j])
                                    {
                                        return false;
                                    }
                                }
                                continue;
                            }
                            else if (genericTypeDefinitionParameters[i].ParameterType.IsGenericParameter && genericArguments.Any(p => p == constructedTypeParameters[i].ParameterType))
                            {
                                continue;
                            }
                            else if (genericTypeDefinitionParameters[i].ParameterType != constructedTypeParameters[i].ParameterType)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                });


            // If inconclusive, just return null
            return (candidateMethods.Count() == 1) ? candidateMethods.First() : null;
        }
    }
}
