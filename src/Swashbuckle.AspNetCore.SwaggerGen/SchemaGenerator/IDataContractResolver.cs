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
        public static DataContract ForPrimitive(
            Type type,
            string dataType,
            string dataFormat = null,
            IEnumerable<object> enumValues = null)
        {
            return new DataContract
            {
                IsPrimitive = true,
                Type = type,
                DataType = dataType,
                DataFormat = dataFormat,
                EnumValues = enumValues,
            };
        }

        public static DataContract ForDictionary(Type type, Type keyType, Type valueType)
        {
            return new DataContract
            {
                IsDictionary = true,
                Type = type,
                DictionaryKeyType = keyType,
                DictionaryValueType = valueType,
                DataType = "object",
            };
        }

        public static DataContract ForArray(Type type, Type itemType)
        {
            return new DataContract
            {
                IsArray = true,
                Type = type,
                ArrayItemType = itemType,
                DataType = "array",
            };
        }

        public static DataContract ForObject(
            Type type,
            IEnumerable<DataMember> members = null,
            Type extensionDataValueType = null)
        {
            return new DataContract
            {
                IsObject = true,
                Type = type,
                Members = members,
                ExtensionDataValueType = extensionDataValueType,
                DataType = "object",
            };
        }

        public static DataContract ForDynamic(Type type)
        {
            return new DataContract
            {
                IsDynamic = true,
                Type = type
            };
        }

        public bool IsPrimitive { get; private set; }

        public bool IsDictionary { get; private set; }

        public bool IsArray { get; private set; }

        public bool IsObject { get; private set; }

        public bool IsDynamic { get; private set; }

        public Type Type { get; private set; }

        public Type DictionaryKeyType { get; private set; }

        public Type DictionaryValueType { get; private set; }

        public Type ArrayItemType { get; private set; }

        public IEnumerable<DataMember> Members { get; private set; }

        public Type ExtensionDataValueType { get; private set; }

        public string DataType { get; private set; }

        public string DataFormat { get; private set; }

        public IEnumerable<object> EnumValues { get; set; }
    }

    public class DataMember
    {
        public DataMember(
            string name,
            Type memberType,
            MemberInfo memberInfo,
            bool isRequired = false,
            bool isNullable = false,
            bool isReadOnly = false,
            bool isWriteOnly = false)
        {
            Name = name;
            MemberType = memberType;
            MemberInfo = memberInfo;
            IsRequired = isRequired;
            IsNullable = isNullable;
            IsReadOnly = isReadOnly;
            IsWriteOnly = isWriteOnly;
        }

        public string Name { get; } 

        public Type MemberType { get; }

        public MemberInfo MemberInfo { get; }

        public bool IsRequired { get; }

        public bool IsNullable { get; }

        public bool IsReadOnly { get; }

        public bool IsWriteOnly { get; }
    }
}