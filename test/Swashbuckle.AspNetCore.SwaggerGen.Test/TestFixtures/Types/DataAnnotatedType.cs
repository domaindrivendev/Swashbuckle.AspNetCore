using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class DataAnnotatedType
    {
        public string StringWithNoAttributes { get; set; }

        [Required]
        public string StringWithRequired { get; set; }

        [Required]
        public int IntWithRequired { get; set; }

        [Range(1, 12)]
        public int IntWithRange { get; set; }

        [RegularExpression("^[3-6]?\\d{12,15}$")]
        public string StringWithRegularExpression { get; set; }

        [StringLength(10, MinimumLength = 5)]
        public string StringWithStringLength { get; set; }

        [MinLength(1), MaxLength(3)]
        public string StringWithMinMaxLength { get; set; }

        [DataType(DataType.Date)]
        public string StringWithDataTypeDate { get; set; }

        [DataType(DataType.DateTime)]
        public string StringWithDataTypeDateTime { get; set; }

        [DataType(DataType.Password)]
        public string StringWithDataTypePassword { get; set; }
    }
}