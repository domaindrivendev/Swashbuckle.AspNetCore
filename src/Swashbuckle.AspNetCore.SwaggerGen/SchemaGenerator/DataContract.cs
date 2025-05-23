namespace Swashbuckle.AspNetCore.SwaggerGen;

public class DataContract
{
    public static DataContract ForPrimitive(
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

    public static DataContract ForArray(
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

    public static DataContract ForDictionary(
        Type underlyingType,
        Type valueType,
        IEnumerable<string> keys = null,
        Func<object, string> jsonConverter = null)
    {
        return new DataContract(
            underlyingType: underlyingType,
            dataType: DataType.Dictionary,
            dictionaryValueType: valueType,
            dictionaryKeys: keys,
            jsonConverter: jsonConverter);
    }

    public static DataContract ForObject(
        Type underlyingType,
        IEnumerable<DataProperty> properties,
        Type extensionDataType = null,
        string typeNameProperty = null,
        string typeNameValue = null,
        Func<object, string> jsonConverter = null)
    {
        return new DataContract(
            underlyingType: underlyingType,
            dataType: DataType.Object,
            objectProperties: properties,
            objectExtensionDataType: extensionDataType,
            objectTypeNameProperty: typeNameProperty,
            objectTypeNameValue: typeNameValue,
            jsonConverter: jsonConverter);
    }

    public static DataContract ForDynamic(
        Type underlyingType,
        Func<object, string> jsonConverter = null)
    {
        return new DataContract(
            underlyingType: underlyingType,
            dataType: DataType.Unknown,
            jsonConverter: jsonConverter);
    }

    private DataContract(
        Type underlyingType,
        DataType dataType,
        string dataFormat = null,
        IEnumerable<object> enumValues = null,
        Type arrayItemType = null,
        Type dictionaryValueType = null,
        IEnumerable<string> dictionaryKeys = null,
        IEnumerable<DataProperty> objectProperties = null,
        Type objectExtensionDataType = null,
        string objectTypeNameProperty = null,
        string objectTypeNameValue = null,
        Func<object, string> jsonConverter = null)
    {
        UnderlyingType = underlyingType;
        DataType = dataType;
        DataFormat = dataFormat;
        ArrayItemType = arrayItemType;
        DictionaryValueType = dictionaryValueType;
        DictionaryKeys = dictionaryKeys;
        ObjectProperties = objectProperties;
        ObjectExtensionDataType = objectExtensionDataType;
        ObjectTypeNameProperty = objectTypeNameProperty;
        ObjectTypeNameValue = objectTypeNameValue;
        JsonConverter = jsonConverter ?? new Func<object, string>(obj => null);
    }

    public Type UnderlyingType { get; }
    public DataType DataType { get; }
    public string DataFormat { get; }
    public Type ArrayItemType { get; }
    public Type DictionaryValueType { get; }
    public IEnumerable<string> DictionaryKeys { get; }
    public IEnumerable<DataProperty> ObjectProperties { get; }
    public Type ObjectExtensionDataType { get; }
    public string ObjectTypeNameProperty { get; }
    public string ObjectTypeNameValue { get; }
    public Func<object, string> JsonConverter { get; }
}
