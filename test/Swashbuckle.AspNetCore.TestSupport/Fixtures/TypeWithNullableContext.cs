using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable

    // These types are used to test our handling of nullable references types.
    // NRT results in NullableContextAttribute and NullableAttribute being placed on
    // types and members by the compiler.

    // Remember to mirror both types and use both types in tests.

    /// <summary>
    /// We expect this type to receive NullableContext(1) (NotAnnotated) from the compiler.
    /// </summary>
    public class TypeWithNullableContextNotAnnotated
    {
        // Dummies to affect the NullableContextAttribute value.
        // It seems to default to the most common nullable state.
        public string Dummy1 { get; } = default!;
        public string Dummy2 { get; } = default!;
        public string Dummy3 { get; } = default!;
        public string Dummy4 { get; } = default!;
        public string Dummy5 { get; } = default!;
        public string Dummy6 { get; } = default!;
        public string Dummy7 { get; } = default!;
        public string Dummy8 { get; } = default!;
        public string Dummy9 { get; } = default!;
        public string Dummy10 { get; } = default!;
        public string Dummy11 { get; set; } = default!;
        public string Dummy12 { get; set; } = default!;
        public string Dummy13 { get; set; } = default!;
        public string Dummy14 { get; set; } = default!;
        public string Dummy15 { get; set; } = default!;
        public string Dummy16 { get; set; } = default!;
        public string Dummy17 { get; set; } = default!;
        public string Dummy18 { get; set; } = default!;
        public string Dummy19 { get; set; } = default!;
        public string Dummy20 { get; set; } = default!;

        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; } = default!;
        public int[]? NullableArray { get; set; }
        public int[] NonNullableArray { get; set; } = default!;

        public List<SubTypeWithOneNullableContent>? NullableList { get; set; }
        public List<SubTypeWithOneNonNullableContent> NonNullableList { get; set; } = default!;

        public Dictionary<string, string>? NullableDictionaryInNonNullableContent { get; set; }
        public Dictionary<string, string> NonNullableDictionaryInNonNullableContent { get; set; } = default!;
        public Dictionary<string, string?> NonNullableDictionaryInNullableContent { get; set; } = default!;
        public Dictionary<string, string?>? NullableDictionaryInNullableContent { get; set; }

        public IDictionary<string, string>? NullableIDictionaryInNonNullableContent { get; set; }
        public IDictionary<string, string> NonNullableIDictionaryInNonNullableContent { get; set; } = default!;
        public IDictionary<string, string?> NonNullableIDictionaryInNullableContent { get; set; } = default!;
        public IDictionary<string, string?>? NullableIDictionaryInNullableContent { get; set; }

        public IReadOnlyDictionary<string, string>? NullableIReadOnlyDictionaryInNonNullableContent { get; set; }
        public IReadOnlyDictionary<string, string> NonNullableIReadOnlyDictionaryInNonNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, string?> NonNullableIReadOnlyDictionaryInNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, string?>? NullableIReadOnlyDictionaryInNullableContent { get; set; }

        public Dictionary<string, int>? NullableDictionaryWithValueTypeInNonNullableContent { get; set; }
        public Dictionary<string, int> NonNullableDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public Dictionary<string, int?> NonNullableDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public Dictionary<string, int?>? NullableDictionaryWithValueTypeInNullableContent { get; set; }

        public IDictionary<string, int>? NullableIDictionaryWithValueTypeInNonNullableContent { get; set; }
        public IDictionary<string, int> NonNullableIDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public IDictionary<string, int?> NonNullableIDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public IDictionary<string, int?>? NullableIDictionaryWithValueTypeInNullableContent { get; set; }

        public IReadOnlyDictionary<string, int>? NullableIReadOnlyDictionaryWithValueTypeInNonNullableContent { get; set; }
        public IReadOnlyDictionary<string, int> NonNullableIReadOnlyDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, int?> NonNullableIReadOnlyDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, int?>? NullableIReadOnlyDictionaryWithValueTypeInNullableContent { get; set; }

        public class SubTypeWithOneNullableContent
        {
            public string? NullableString { get; set; }
        }

        public class SubTypeWithOneNonNullableContent
        {
            public string NonNullableString { get; set; } = default!;
        }
    }

    /// <summary>
    /// We expect this type to receive NullableContext(2) (Annotated) from the compiler.
    /// </summary>
    public class TypeWithNullableContextAnnotated
    {
        // Dummies to affect the NullableContextAttribute value.
        // It seems to default to the most common nullable state.
        public string? Dummy1 { get; set; }
        public string? Dummy2 { get; set; }
        public string? Dummy3 { get; set; }
        public string? Dummy4 { get; set; }
        public string? Dummy5 { get; set; }
        public string? Dummy6 { get; set; }
        public string? Dummy7 { get; set; }
        public string? Dummy8 { get; set; }
        public string? Dummy9 { get; set; }
        public string? Dummy10 { get; set; }
        public string? Dummy11 { get; set; }
        public string? Dummy12 { get; set; }
        public string? Dummy13 { get; set; }
        public string? Dummy14 { get; set; }
        public string? Dummy15 { get; set; }
        public string? Dummy16 { get; set; }
        public string? Dummy17 { get; set; }
        public string? Dummy18 { get; set; }
        public string? Dummy19 { get; set; }
        public string? Dummy20 { get; set; }

        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; } = default!;
        public int[]? NullableArray { get; set; }
        public int[] NonNullableArray { get; set; } = default!;

        public List<SubTypeWithOneNullableContent>? NullableList { get; set; }
        public List<SubTypeWithOneNonNullableContent> NonNullableList { get; set; } = default!;

        public Dictionary<string, string>? NullableDictionaryInNonNullableContent { get; set; }
        public Dictionary<string, string> NonNullableDictionaryInNonNullableContent { get; set; } = default!;
        public Dictionary<string, string?> NonNullableDictionaryInNullableContent { get; set; } = default!;
        public Dictionary<string, string?>? NullableDictionaryInNullableContent { get; set; }

        public IDictionary<string, string>? NullableIDictionaryInNonNullableContent { get; set; }
        public IDictionary<string, string> NonNullableIDictionaryInNonNullableContent { get; set; } = default!;
        public IDictionary<string, string?> NonNullableIDictionaryInNullableContent { get; set; } = default!;
        public IDictionary<string, string?>? NullableIDictionaryInNullableContent { get; set; }

        public IReadOnlyDictionary<string, string>? NullableIReadOnlyDictionaryInNonNullableContent { get; set; }
        public IReadOnlyDictionary<string, string> NonNullableIReadOnlyDictionaryInNonNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, string?> NonNullableIReadOnlyDictionaryInNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, string?>? NullableIReadOnlyDictionaryInNullableContent { get; set; }

        public Dictionary<string, int>? NullableDictionaryWithValueTypeInNonNullableContent { get; set; }
        public Dictionary<string, int> NonNullableDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public Dictionary<string, int?> NonNullableDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public Dictionary<string, int?>? NullableDictionaryWithValueTypeInNullableContent { get; set; }

        public IDictionary<string, int>? NullableIDictionaryWithValueTypeInNonNullableContent { get; set; }
        public IDictionary<string, int> NonNullableIDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public IDictionary<string, int?> NonNullableIDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public IDictionary<string, int?>? NullableIDictionaryWithValueTypeInNullableContent { get; set; }

        public IReadOnlyDictionary<string, int>? NullableIReadOnlyDictionaryWithValueTypeInNonNullableContent { get; set; }
        public IReadOnlyDictionary<string, int> NonNullableIReadOnlyDictionaryWithValueTypeInNonNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, int?> NonNullableIReadOnlyDictionaryWithValueTypeInNullableContent { get; set; } = default!;
        public IReadOnlyDictionary<string, int?>? NullableIReadOnlyDictionaryWithValueTypeInNullableContent { get; set; }

        public class SubTypeWithOneNullableContent
        {
            public string? NullableString { get; set; }
        }

        public class SubTypeWithOneNonNullableContent
        {
            public string NonNullableString { get; set; } = default!;
        }
    }
#nullable restore
}
