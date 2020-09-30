using System.ComponentModel;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class EnumDefaultValueAnnotatedType
    {
        [DefaultValue(IntEnum.Value8)]
        public IntEnum IntEnumWithDefaultValue { get; set; }

        [DefaultValue(JsonConverterAnnotatedEnum.X)]
        public JsonConverterAnnotatedEnum AnnotatedEnumWithDefaultValue { get; set; }
    }
}
