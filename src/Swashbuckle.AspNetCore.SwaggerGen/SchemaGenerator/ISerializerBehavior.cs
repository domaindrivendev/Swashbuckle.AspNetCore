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
        public static DataContract ForPrimitive(Type underlyingType, DataType dataType, string dataFormat)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: dataType,
                dataFormat: dataFormat);
        }

        public static DataContract ForArray(Type underlyingType, Type itemType)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Array,
                arrayItemType: itemType);
        }

        public static DataContract ForDictionary(Type underlyingType, Type keyType, Type valueType)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Dictionary,
                dictionaryKeyType: keyType,
                dictionaryValueType: valueType);
        }

        public static DataContract ForObject(
            Type underlyingType,
            IEnumerable<DataProperty> properties,
            Type extensionDataType = null,
            string typeNameProperty = null,
            string typeNameValue = null)
        {
            return new DataContract(
                underlyingType: underlyingType,
                dataType: DataType.Object,
                objectProperties: properties,
                objectExtensionDataType: extensionDataType,
                objectTypeNameProperty: typeNameProperty,
                objectTypeNameValue: typeNameValue);
        }

        public static DataContract ForDynamic(Type underlyingType)
        {
            return new DataContract(underlyingType: underlyingType, dataType: DataType.Unknown);
        }

        private DataContract(
            Type underlyingType,
            DataType dataType,
            string dataFormat = null,
            Type arrayItemType = null,
            Type dictionaryKeyType = null,
            Type dictionaryValueType = null,
            IEnumerable<string> dictionaryKeys = null,
            IEnumerable<DataProperty> objectProperties = null,
            Type objectExtensionDataType = null,
            string objectTypeNameProperty = null,
            string objectTypeNameValue = null)
        {
            UnderlyingType = underlyingType;
            DataType = dataType;
            DataFormat = dataFormat;
            ArrayItemType = arrayItemType;
            DictionaryKeyType = dictionaryKeyType;
            DictionaryValueType = dictionaryValueType;
            ObjectProperties = objectProperties;
            ObjectExtensionDataType = objectExtensionDataType;
            ObjectTypeNameProperty = objectTypeNameProperty;
            ObjectTypeNameValue = objectTypeNameValue;
        }

        public Type UnderlyingType { get; }
        public DataType DataType { get; }
        public string DataFormat { get; }
        public Type ArrayItemType { get; }
        public Type DictionaryKeyType { get; }
        public Type DictionaryValueType { get; }
        public IEnumerable<DataProperty> ObjectProperties { get; }
        public Type ObjectExtensionDataType { get; }
        public string ObjectTypeNameProperty { get; }
        public string ObjectTypeNameValue { get; }
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