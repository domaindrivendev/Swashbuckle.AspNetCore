using System.Globalization;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public struct TestValueType
    {
        public TestValueType(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public override string ToString() => Value.ToString("x8");

        public static TestValueType Parse(string input) => new TestValueType(int.Parse(input, NumberStyles.HexNumber));
    }
}