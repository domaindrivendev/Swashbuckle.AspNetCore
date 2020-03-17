using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISerializerMetadataResolver
    {
        SerializerMetadata GetSerializerMetadataForType(Type type);
    }

    public class SerializerMetadata
    {
        public static SerializerMetadata ForPrimitive(
            Type type,
            string dataType,
            string dataFormat = null,
            IEnumerable<object> enumValues = null)
        {
            return new SerializerMetadata
            {
                IsPrimitive = true,
                Type = type,
                DataType = dataType,
                DataFormat = dataFormat,
                EnumValues = enumValues,
            };
        }

        public static SerializerMetadata ForDictionary(Type type, Type keyType, Type valueType)
        {
            return new SerializerMetadata
            {
                IsDictionary = true,
                Type = type,
                DictionaryKeyType = keyType,
                DictionaryValueType = valueType,
                DataType = "object",
            };
        }

        public static SerializerMetadata ForArray(Type type, Type itemType)
        {
            return new SerializerMetadata
            {
                IsArray = true,
                Type = type,
                ArrayItemType = itemType,
                DataType = "array",
            };
        }

        public static SerializerMetadata ForObject(
            Type type,
            IEnumerable<SerializerPropertyMetadata> properties = null,
            Type extensionDataValueType = null)
        {
            return new SerializerMetadata
            {
                IsObject = true,
                Type = type,
                Properties = properties,
                ExtensionDataValueType = extensionDataValueType,
                DataType = "object",
            };
        }

        public static SerializerMetadata ForDynamic(Type type)
        {
            return new SerializerMetadata
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

        public IEnumerable<SerializerPropertyMetadata> Properties { get; private set; }

        public Type ExtensionDataValueType { get; private set; }

        public string DataType { get; private set; }

        public string DataFormat { get; private set; }

        public IEnumerable<object> EnumValues { get; set; }
    }

    public class SerializerPropertyMetadata
    {
        public SerializerPropertyMetadata(
            string name,
            Type memberType,
            MemberInfo memberInfo,
            bool isRequired = false,
            bool allowNull = false)
        {
            Name = name;
            MemberType = memberType;
            MemberInfo = memberInfo;
            IsRequired = isRequired;
            AllowNull = allowNull;
        }

        public string Name { get; } 

        public Type MemberType { get; }

        public MemberInfo MemberInfo { get; }

        public bool IsRequired { get; }

        public bool AllowNull { get; }
    }
}