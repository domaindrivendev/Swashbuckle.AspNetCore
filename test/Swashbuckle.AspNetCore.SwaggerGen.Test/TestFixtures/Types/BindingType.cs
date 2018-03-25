using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class BindingType
    {
        public string Property1 { get; set; }

        [BindNever]
        public string Property2 { get; set; }

        [BindRequired]
        public string Property3 { get; set; }
    }
}
