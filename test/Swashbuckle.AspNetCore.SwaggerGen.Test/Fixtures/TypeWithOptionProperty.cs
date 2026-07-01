namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures;

#nullable enable

public class TypeWithOptionProperty
{
    public int Int { get; set; }
    public int? NullableInt { get; set; }
    public OptionalValue<int> OptionalInt { get; set; }
    public OptionalValue<int?> NullableOptionalInt { get; set; }

    public string String { get; set; } = null!;
    public string? NullableString { get; set; }
    public OptionalValue<string> OptionalString { get; set; }
    public OptionalValue<string?> NullableOptionalString { get; set; }

    public TypeWithOptionProperty Self { get; set; } = null!;
    public TypeWithOptionProperty? NullableSelf { get; set; }
    public OptionalValue<TypeWithOptionProperty> OptionalSelf { get; set; }
    public OptionalValue<TypeWithOptionProperty?> NullableOptionalSelf { get; set; }
}
