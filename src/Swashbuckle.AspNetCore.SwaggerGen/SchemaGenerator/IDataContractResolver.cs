using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IDataContractResolver
    {
        DataContract GetDataContractForType(Type type);
    }

    public class DataContract
    {
        public DataContract(
            DataType dataType,
            Type underlyingType,
            string format = null,
            IEnumerable<object> enumValues = null,
            IEnumerable<DataProperty> properties = null,
            Type additionalPropertiesType = null,
            Type arrayItemType = null)
        {
            DataType = dataType;
            Format = format;
            EnumValues = enumValues;
            Properties = properties;
            UnderlyingType = underlyingType;
            AdditionalPropertiesType = additionalPropertiesType;
            ArrayItemType = arrayItemType;
        }

        public DataType DataType { get; }
        public string Format { get; }
        public IEnumerable<object> EnumValues { get; }
        public IEnumerable<DataProperty> Properties { get; }
        public Type UnderlyingType { get; }
        public Type AdditionalPropertiesType { get; }
        public Type ArrayItemType { get; }
    }

    public enum DataType
    {
        Unknown,
        Boolean,
        Integer,
        Number,
        String,
        Object,
        Array
    }

    public class DataProperty
    {
        public DataProperty(
            string name,
            Type memberType,
            bool isRequired = false,
            bool isNullable = false,
            bool isReadOnly = false,
            bool isWriteOnly = false,
            MemberInfo memberInfo = null)
        {
            Name = name;
            IsRequired = isRequired;
            IsNullable = isNullable;
            IsReadOnly = isReadOnly;
            IsWriteOnly = isWriteOnly;
            MemberType = memberType;
            MemberInfo = memberInfo;
        }

        public string Name { get; } 

        public bool IsRequired { get; }

        public bool IsNullable { get; }

        public bool IsReadOnly { get; }

        public bool IsWriteOnly { get; }

        public Type MemberType { get; }

        public MemberInfo MemberInfo { get; }
    }
}