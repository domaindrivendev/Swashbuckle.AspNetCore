using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithValidationAttributes
    {
        [DataType(DataType.CreditCard)]
        public string StringWithDataTypeCreditCard { get; set; }

        [MinLength(1), MaxLength(3)]
        public string StringWithMinMaxLength { get; set; }

        [MinLength(1), MaxLength(3)]
        public string[] ArrayWithMinMaxLength { get; set; }

        [Range(1, 10)]
        public int IntWithRange { get; set; }

        [RegularExpression("^[3-6]?\\d{12,15}$")]
        public string StringWithRegularExpression { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string StringWithStringLength { get; set; }

        [Required]
        public string StringWithRequired { get; set; }
    }
}