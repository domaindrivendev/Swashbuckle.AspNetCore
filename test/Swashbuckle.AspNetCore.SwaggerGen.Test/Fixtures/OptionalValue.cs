namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public readonly struct OptionalValue<T>
{
    public OptionalValue()
    {
        HasValue = false;
    }

    public OptionalValue(T value)
    {
        Value = value;
        HasValue = true;
    }

    public T Value { get; }

    public bool HasValue { get; }
}
