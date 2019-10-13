using Microsoft.AspNetCore.Mvc.ModelBinding;

#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;
#endif

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class ModelMetadataHelper
    {
        public static IModelMetadataProvider GetDefaultModelMetadataProvider()
        {
#if NETCOREAPP3_0
            return new EmptyModelMetadataProvider();
#else
            return new DefaultModelMetadataProvider(
                new DefaultCompositeMetadataDetailsProvider(new IMetadataDetailsProvider[]
                {
                    new DefaultBindingMetadataProvider(),
                    new DefaultValidationMetadataProvider(),
                    new DataAnnotationsMetadataProvider(Options.Create(new MvcDataAnnotationsLocalizationOptions()), null),
                }
            ));
#endif
        }
    }
}
