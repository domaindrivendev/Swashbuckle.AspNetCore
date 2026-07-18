using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.TestSupport;

#nullable enable

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

// See https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/3936
public class TypeWithNullableObjectDictionary
{
    public IDictionary<string, object?> Property { get; set; } = default!;
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
