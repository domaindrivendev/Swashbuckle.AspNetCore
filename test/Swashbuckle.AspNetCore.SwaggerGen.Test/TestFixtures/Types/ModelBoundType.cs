using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class ModelBoundType
    {
        public string StringWithNoAttributes { get; set; }

        [BindRequired]
        public string StringWithBindRequired { get; set; }

        [BindRequired]
        public int IntWithBindRequired { get; set; }

        [BindNever]
        public int IntWithBindNever { get; set; }
    }
}
