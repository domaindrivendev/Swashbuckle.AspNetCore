namespace Swashbuckle.AspNetCore.SwaggerGen;

#nullable enable

public static class MemberInfoExtensionsTests
{
    [Theory]
    [InlineData(typeof(MyClass), nameof(MyClass.DictionaryInt32NonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.DictionaryInt32Nullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.DictionaryStringNonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.DictionaryStringNullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.IDictionaryInt32NonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.IDictionaryInt32Nullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.IDictionaryStringNonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.IDictionaryStringNullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.IReadOnlyDictionaryInt32NonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.IReadOnlyDictionaryInt32Nullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.IReadOnlyDictionaryStringNonNullable), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.IReadOnlyDictionaryStringNullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.StringDictionary), false)] // There is no way to inspect the nullability of the base class' TValue argument
    [InlineData(typeof(MyClass), nameof(MyClass.NullableStringDictionary), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.SameTypesDictionary), true)]
    [InlineData(typeof(MyClass), nameof(MyClass.CustomDictionaryStringNullable), false)]
    [InlineData(typeof(MyClass), nameof(MyClass.CustomDictionaryStringNonNullable), true)]
    public static void IsDictionaryValueNonNullable_Returns_Correct_Value(Type type, string memberName, bool expected)
    {
        // Arrange
        var memberInfo = type.GetMember(memberName).First();

        // Act
        var actual = memberInfo.IsDictionaryValueNonNullable();

        // Assert
        Assert.Equal(expected, actual);
    }

    public class MyClass
    {
        public Dictionary<string, int> DictionaryInt32NonNullable { get; set; } = [];

        public Dictionary<string, int?> DictionaryInt32Nullable { get; set; } = [];

        public Dictionary<string, string> DictionaryStringNonNullable { get; set; } = [];

        public Dictionary<string, string?> DictionaryStringNullable { get; set; } = [];

        public IDictionary<string, int> IDictionaryInt32NonNullable { get; set; } = new Dictionary<string, int>();

        public IDictionary<string, int?> IDictionaryInt32Nullable { get; set; } = new Dictionary<string, int?>();

        public IDictionary<string, string> IDictionaryStringNonNullable { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string?> IDictionaryStringNullable { get; set; } = new Dictionary<string, string?>();

        public IReadOnlyDictionary<string, int> IReadOnlyDictionaryInt32NonNullable { get; set; } = new Dictionary<string, int>();

        public IReadOnlyDictionary<string, int?> IReadOnlyDictionaryInt32Nullable { get; set; } = new Dictionary<string, int?>();

        public IReadOnlyDictionary<string, string> IReadOnlyDictionaryStringNonNullable { get; set; } = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string?> IReadOnlyDictionaryStringNullable { get; set; } = new Dictionary<string, string?>();

        public StringDictionary StringDictionary { get; set; } = [];

        public NullableStringDictionary NullableStringDictionary { get; set; } = [];

        public SameTypesDictionary<string> SameTypesDictionary { get; set; } = [];

        public CustomDictionary<string, string?> CustomDictionaryStringNullable { get; set; } = [];

        public CustomDictionary<string, string> CustomDictionaryStringNonNullable { get; set; } = [];
    }

    public class StringDictionary : Dictionary<string, string>;

    public class NullableStringDictionary : Dictionary<string, string?>;

    public class SameTypesDictionary<T> : Dictionary<T, T> where T : notnull;

    public class CustomDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull;
}
