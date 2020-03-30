using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.TestSupport
{
    [ModelMetadataType(typeof(MetadataType))]
    public class DataAnnotatedViaMetadataType
    {
        public string StringWithRequired { get; set; }

        public int IntWithRequired { get; set; }

        public int IntWithRange { get; set; }

        public string StringWithRegularExpression { get; set; }
    }

    public class MetadataType
    {
        [Required]
        public string StringWithRequired { get; set; }

        [Required]
        public int IntWithRequired { get; set; }

        [Range(1, 12)]
        public int IntWithRange { get; set; }

        [RegularExpression("^[3-6]?\\d{12,15}$")]
        public string StringWithRegularExpression { get; set; }
    }
}