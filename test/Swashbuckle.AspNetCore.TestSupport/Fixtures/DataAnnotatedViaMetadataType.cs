using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.TestSupport.Fixtures;

namespace Swashbuckle.AspNetCore.TestSupport
{
    [ModelMetadataType(typeof(MetadataType))]
    public class DataAnnotatedViaMetadataType
    {
        public string StringWithRequired { get; set; }

        public int IntWithRequired { get; set; }

        public int IntWithRange { get; set; }

        public string StringWithRegularExpression { get; set; }

        [DefaultValue("foobar")]
        public string StringWithDefaultValue { get; set; }

        [Range(1, 20)]
        public int IntWithTwoRanges { get; set; }

        [Range(6, 42)]
        public int IntWithRangeSubclass { get; set; }
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

        // NOTE: Annotation for this one is on the actual model
        public string StringWithDefaultValue { get; set; }

        // NOTE: Redeclared, but with different values - the model's version should win over this one
        [Range(100, 2000)]
        public int IntWithTwoRanges { get; set; }

        // NOTE: A subclass of the model's attribute - the model's version should win over this one
        [ExtendedRange(600, 4200)]
        public int IntWithRangeSubclass { get; set; }
    }
}