using System;
using System.Collections.Generic;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISerializerBehavior
    {
        DataContract GetDataContractForType(Type type);

        string Serialize(object value);
    }

    public class DataContract
    {
        public static DataContract Primitive(
            Type underlyingType,
            DataType dataType,
            string dataFormat,
            Func<object, string> jsonConverter = null)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: dataType,
                dataFormat: dataFormat,
                jsonConverter: jsonConverter);
        }

        public static DataContract Array(
            Type underlyingType,
            Type itemType,
            Func<object, string> jsonConverter = null)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Array,
                arrayItemType: itemType,
                jsonConverter: jsonConverter);
        }

        public static DataContract Dictionary(
            Type underlyingType,
            Type keyType,
            Type valueType,
            Func<object, string> jsonConverter = null)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Dictionary,
                dictionaryKeyType: keyType,
                dictionaryValueType: valueType,
                jsonConverter: jsonConverter);
        }

        public static DataContract Object(
            Type underlyingType,
            IEnumerable<DataProperty> properties,
            Type extensionDataType = null,
            string discriminatorPropertyName = null,
            string discriminatorPropertyValue = null,
            Func<object, string> jsonConverter = null)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Object,
                objectProperties: properties,
                objectExtensionDataType: extensionDataType,
                objectDiscriminatorProperty: discriminatorPropertyName,
                objectDiscriminatorValue: discriminatorPropertyValue,
                jsonConverter: jsonConverter);
        }

        public static DataContract Undefined(Type underlyingType)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Unknown);
        }

        private DataContract(
            Type underlyingType,
            DataType dataType,
            string dataFormat = null,
            Type arrayItemType = null,
            Type dictionaryKeyType = null,
            Type dictionaryValueType = null,
            IEnumerable<DataProperty> objectProperties = null,
            Type objectExtensionDataType = null,
            string objectDiscriminatorProperty = null,
            string objectDiscriminatorValue = null,
            Func<object, string> jsonConverter = null)
        {
            UnderlyingType = underlyingType;
            DataType = dataType;
            DataFormat = dataFormat;
            ArrayItemType = arrayItemType;
            DictionaryKeyType = dictionaryKeyType;
            DictionaryValueType = dictionaryValueType;
            ObjectProperties = objectProperties;
            ObjectExtensionDataType = objectExtensionDataType;
            ObjectDiscriminatorProperty = objectDiscriminatorProperty;
            ObjectDiscriminatorValue = objectDiscriminatorValue;
            JsonConverter = jsonConverter;
        }

        public Type UnderlyingType { get; }
        public DataType DataType { get; }
        public string DataFormat { get; }
        public Type ArrayItemType { get; }
        public Type DictionaryKeyType { get; }
        public Type DictionaryValueType { get; }
        public IEnumerable<DataProperty> ObjectProperties { get; }
        public Type ObjectExtensionDataType { get; }
        public string ObjectDiscriminatorProperty { get; }
        public string ObjectDiscriminatorValue { get; }
        public Func<object, string> JsonConverter { get; }
    }

    public enum DataType
    {
        Boolean,
        Integer,
        Number,
        String,
        Array,
        Dictionary,
        Object,
        Unknown
    }

    public class DataProperty
    {
        public DataProperty(
            string name,
            MemberInfo memberInfo,
            bool isRequired = false,
            bool isNullable = false,
            bool isReadOnly = false,
            bool isWriteOnly = false)
        {
            Name = name;
            MemberInfo = memberInfo;
            IsRequired = isRequired;
            IsNullable = isNullable;
            IsReadOnly = isReadOnly;
            IsWriteOnly = isWriteOnly;
        }

        public string Name { get; } 
        public MemberInfo MemberInfo { get; }
        public bool IsRequired { get; }
        public bool IsNullable { get; }
        public bool IsReadOnly { get; }
        public bool IsWriteOnly { get; }
    }
}