using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.TestSupport
{
    [ModelMetadataType(typeof(MetadataType))]
    public class TypeWithValidationAttributesViaMetadataType
    {
        public string StringWithDataTypeCreditCard { get; set; }

        public string StringWithMinMaxLength { get; set; }

        public string[] ArrayWithMinMaxLength { get; set; }

        public int IntWithRange { get; set; }

        public string StringWithRegularExpression { get; set; }

        public string StringWithStringLength { get; set; }

        [Required]
        public string StringWithRequired { get; set; }
    }

    public class MetadataType
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

        // NOTE: RequiredAttribute for this is applied in the actual type
        public string StringWithRequired { get; set; }
    }
}