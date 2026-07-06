using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;

namespace Swashbuckle.AspNetCore.TestSupport;

#nullable enable

public record TestRecordStringInteger(IDictionary<string, int> Property);

public record TestRecordStringIntegerNullable(IDictionary<string, int?> Property);

public record TestRecordEmptyIntEnumInteger(IDictionary<EmptyIntEnum, int> Property);

public record TestRecordStringBoolean(IDictionary<string, bool> Property);

public record TestRecordStringBooleanNullable(IDictionary<string, bool?> Property);

public record TestRecordDictionary(IDictionary Property);

public record TestRecordStringObject(IDictionary<string, object> Property);

public record TestRecordStringObjectNullable(IDictionary<string, object?> Property);

public record TestRecordExpandoObject(ExpandoObject Property);

public record TestRecordStringComplexType(IDictionary<string, ComplexType> Property);

public record TestRecordStringComplexTypeNullable(IDictionary<string, ComplexType?> Property);

public class CustomAnyOfType;

public class CustomAllOfType;

public class CustomOneOfType;

public class TypeWithNullableCustomAnyOfProperty
{
    public CustomAnyOfType? Property { get; set; }
}

public class TypeWithNullableCustomAllOfProperty
{
    public CustomAllOfType? Property { get; set; }
}

public class TypeWithNullableCustomOneOfProperty
{
    public CustomOneOfType? Property { get; set; }
}

public class TypeWithRequiredProperties
{
    public required string RequiredString { get; set; }

    public required int RequiredInt { get; set; }
}

public class TypeWithRequiredPropertyAndValidationAttribute
{
    [MinLength(1)]
    public required string RequiredProperty { get; set; }
}

public class TypeWithNullableReferenceTypes
{
    public required string? RequiredNullableString { get; set; }

    public required string RequiredNonNullableString { get; set; }
}

#nullable restore
