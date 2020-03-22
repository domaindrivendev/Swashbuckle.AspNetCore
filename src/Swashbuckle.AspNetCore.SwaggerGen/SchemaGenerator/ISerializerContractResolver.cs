using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISerializerContractResolver
    {
        SerializerContract GetSerializerContractForType(Type type);
    }

    public class SerializerContract
    {
        public static SerializerContract ForPrimitive(
            Type type,
            string dataType,
            string dataFormat = null,
            IEnumerable<object> enumValues = null)
        {
            return new SerializerContract
            {
                IsPrimitive = true,
                Type = type,
                DataType = dataType,
                DataFormat = dataFormat,
                EnumValues = enumValues,
            };
        }

        public static SerializerContract ForDictionary(Type type, Type keyType, Type valueType)
        {
            return new SerializerContract
            {
                IsDictionary = true,
                Type = type,
                DictionaryKeyType = keyType,
                DictionaryValueType = valueType,
                DataType = "object",
            };
        }

        public static SerializerContract ForArray(Type type, Type itemType)
        {
            return new SerializerContract
            {
                IsArray = true,
                Type = type,
                ArrayItemType = itemType,
                DataType = "array",
            };
        }

        public static SerializerContract ForObject(
            Type type,
            IEnumerable<SerializerMember> members = null,
            Type extensionDataValueType = null)
        {
            return new SerializerContract
            {
                IsObject = true,
                Type = type,
                Members = members,
                ExtensionDataValueType = extensionDataValueType,
                DataType = "object",
            };
        }

        public static SerializerContract ForDynamic(Type type)
        {
            return new SerializerContract
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

        public IEnumerable<SerializerMember> Members { get; private set; }

        public Type ExtensionDataValueType { get; private set; }

        public string DataType { get; private set; }

        public string DataFormat { get; private set; }

        public IEnumerable<object> EnumValues { get; set; }
    }

    public class SerializerMember
    {
        public SerializerMember(
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