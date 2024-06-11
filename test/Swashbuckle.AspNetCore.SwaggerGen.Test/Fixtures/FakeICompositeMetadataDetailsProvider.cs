using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test.Fixtures
{
    internal class FakeICompositeMetadataDetailsProvider : ICompositeMetadataDetailsProvider
    {
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            context.BindingMetadata.IsBindingAllowed = true;
        }

        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            throw new NotImplementedException();
        }

        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            throw new NotImplementedException();
        }
    }
}
