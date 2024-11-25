using System.ComponentModel;
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

#if NET8_0_OR_GREATER

        [Length(1, 3)]
        public string StringWithLength { get; set; }

        [Length(1, 3)]
        public string[] ArrayWithLength { get; set; }

        [Range(1, 10, MinimumIsExclusive = true, MaximumIsExclusive = true)]
        public int IntWithExclusiveRange { get; set; }

        [Base64String]
        public string StringWithBase64 { get; set; }

#endif

        [Range(1, 10)]
        public int IntWithRange { get; set; }

        [RegularExpression("^[3-6]?\\d{12,15}$")]
        public string StringWithRegularExpression { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string StringWithStringLength { get; set; }

        [Required]
        public string StringWithRequired { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string StringWithRequiredAllowEmptyTrue { get; set; }

        [Description("Description")]
        public string StringWithDescription { get; set; }

        [ReadOnly(true)]
        public string StringWithReadOnly { get; set; }
    }
}
