using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public sealed class OptionalValueDataContractResolver(
    ISerializerDataContractResolver innerResolver,
    SchemaGeneratorOptions generatorOptions
) : ISerializerDataContractResolver
{
    public DataContract GetDataContractForType(Type type)
    {
        Type effectiveType = type;
        if (IsOptionType(type, out var underlyingType))
        {
            effectiveType = underlyingType;
        }

        var dataContract = innerResolver.GetDataContractForType(effectiveType);
        if (dataContract.DataType != DataType.Object)
        {
            return dataContract;
        }

        if (dataContract is { DataType: DataType.Object })
        {
            var effectiveProperties = new List<DataProperty>();
            foreach (DataProperty property in dataContract.ObjectProperties)
            {
                DataProperty effectiveProperty = property;
                if (IsOptionType(property.MemberType, out underlyingType))
                {
                    // By default an OptionalValue<T> is treated as nullable.
                    var isNullable = true;
                    if (generatorOptions.SupportNonNullableReferenceTypes)
                    {
                        var nullabilityInfo = GetNullabilityInfo(property.MemberInfo);
                        // set the nullability here when enabled
                        var underlyingNullabilityInfo = nullabilityInfo?.GenericTypeArguments.FirstOrDefault();
                        isNullable = underlyingNullabilityInfo?.ReadState == NullabilityState.Nullable;
                    }

                    effectiveProperty = new DataProperty(
                        name: property.Name,
                        memberType: underlyingType,
                        isRequired: false,
                        isNullable: isNullable,
                        isReadOnly: property.IsReadOnly,
                        isWriteOnly: property.IsWriteOnly,
                        memberInfo: property.MemberInfo);
                }

                effectiveProperties.Add(effectiveProperty);
            }

            dataContract = DataContract.ForObject(
                dataContract.UnderlyingType,
                effectiveProperties,
                dataContract.ObjectExtensionDataType,
                dataContract.ObjectTypeNameProperty,
                dataContract.ObjectTypeNameProperty,
                dataContract.JsonConverter);
        }

        return dataContract;
    }

    private static bool IsOptionType(Type type, out Type underlyingType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OptionalValue<>))
        {
            underlyingType = type.GetGenericArguments()[0];
            return true;
        }

        underlyingType = null;
        return false;
    }

    private static NullabilityInfo GetNullabilityInfo(ICustomAttributeProvider memberInfo)
    {
        var nullabilityInfoContext = new NullabilityInfoContext();
        return memberInfo switch
        {
            PropertyInfo propertyInfo => nullabilityInfoContext.Create(propertyInfo),
            FieldInfo fieldInfo => nullabilityInfoContext.Create(fieldInfo),
            ParameterInfo parameterInfo => nullabilityInfoContext.Create(parameterInfo),
            _ => null,
        };
    }
}
