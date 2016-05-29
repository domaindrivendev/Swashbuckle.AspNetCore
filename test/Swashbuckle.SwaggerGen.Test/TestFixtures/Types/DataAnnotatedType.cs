using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class DataAnnotatedType
    {
        [Required, Range(1, 12)]
        public int RangeProperty { get; set; }

        [Required, RegularExpression("^[3-6]?\\d{12,15}$")]
        public string PatternProperty { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string StringProperty1 { get; set; }

        [MinLength(1), MaxLength(3)]
        public string StringProperty2 { get; set; }

        public string OptionalProperty { get; set; }
    }
}