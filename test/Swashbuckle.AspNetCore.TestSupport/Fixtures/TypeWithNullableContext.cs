using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable
    public class TypeWithNullableContext
    {
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
