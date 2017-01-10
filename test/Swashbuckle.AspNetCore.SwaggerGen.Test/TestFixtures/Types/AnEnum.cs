using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public enum AnEnum
    {
        Value1 = 2,
        [Display(Name = "Value 2")]
        Value2 = 4,
        X = 8
    }
}