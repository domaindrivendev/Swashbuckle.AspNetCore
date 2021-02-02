using System.ComponentModel;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithDefaultAttributes
    {
        [DefaultValue(true)]
        public bool BoolWithDefault { get; set; }

        [DefaultValue(int.MaxValue)]
        public int IntWithDefault { get; set; }

        [DefaultValue(long.MaxValue)]
        public long LongWithDefault { get; set; }

        [DefaultValue(float.MaxValue)]
        public float FloatWithDefault { get; set; }

        [DefaultValue(double.MaxValue)]
        public double DoubleWithDefault { get; set; }

        [DefaultValue("foobar")]
        public string StringWithDefault { get; set; }

        [DefaultValue(new[] { 1, 2, 3 })]
        public int[] IntArrayWithDefault { get; set; }

        [DefaultValue(new[] { "foo", "bar" })]
        public string[] StringArrayWithDefault { get; set; }
    }
}
