namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithPropertySetViaConstructor
    {
        public TypeWithPropertySetViaConstructor(bool readOnlyProperty)
        {
            ReadOnlyProperty = readOnlyProperty;
        }

        public bool ReadOnlyProperty { get; }
    }
}
