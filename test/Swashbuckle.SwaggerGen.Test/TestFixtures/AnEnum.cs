using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public enum AnEnum
    {
        Value1 = 2,
        [Display(Name = "Value 2")]
        Value2 = 4,
        X = 8
    }
}