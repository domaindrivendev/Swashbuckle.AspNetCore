using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [ModelMetadataType(typeof(MetadataType))]
    public class MetadataAnnotatedType
    {
        public int RangeProperty { get; set; }

        public string PatternProperty { get; set; }
    }

    public class MetadataType
    {
        [Required, Range(1, 12)]
        public int RangeProperty { get; set; }

        [Required, RegularExpression("^[3-6]?\\d{12,15}$")]
        public string PatternProperty { get; set; }
    }

}