using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

#if NETCOREAPP2_0
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using Microsoft.AspNetCore.Mvc.Internal;
#endif

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class ModelMetadataHelper
    {
        public static IModelMetadataProvider GetDefaultModelMetadataProvider()
        {
#if NETCOREAPP2_0
            return new DefaultModelMetadataProvider(
                new DefaultCompositeMetadataDetailsProvider(new IMetadataDetailsProvider[]
                {
                    new DefaultBindingMetadataProvider(),
                    new DefaultValidationMetadataProvider(),
                    new DataAnnotationsMetadataProvider(Options.Create(new MvcDataAnnotationsLocalizationOptions()), null),
                }
            ));
#else
            return new EmptyModelMetadataProvider();
#endif
        }
    }
}
