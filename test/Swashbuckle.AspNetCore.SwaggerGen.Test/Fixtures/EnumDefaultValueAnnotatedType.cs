using System.ComponentModel;
using Swashbuckle.AspNetCore.TestSupport;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class EnumDefaultValueAnnotatedType
    {
        [DefaultValue(IntEnum.Value8)]
        public IntEnum IntEnumWithDefaultValue { get; set; }
    }
}