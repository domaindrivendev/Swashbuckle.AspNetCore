using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ModelBinding;
using Moq;

namespace Swashbuckle.Fixtures.ApiDescriptions
{
    public class FakeApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly List<ControllerActionDescriptor> _actionDescriptors;

        public FakeApiDescriptionGroupCollectionProvider()
        {
            _actionDescriptors = new List<ControllerActionDescriptor>();
        }

        public FakeApiDescriptionGroupCollectionProvider Add(
            string httpMethod, string routeTemplate, string actionFixtureName)
        {
            _actionDescriptors.Add(CreateActionDescriptor(httpMethod, routeTemplate, actionFixtureName));
            return this;
        }

        public ApiDescriptionGroupCollection ApiDescriptionGroups
        {
            get
            {
                var apiDescriptions = GetApiDescriptions();
                var group = new ApiDescriptionGroup("default", apiDescriptions);
                return new ApiDescriptionGroupCollection(new[] { group }, 1);
            }
        }

        private ControllerActionDescriptor CreateActionDescriptor(
            string httpMethod, string routeTemplate, string actionFixtureName)
        {
            var descriptor = new ControllerActionDescriptor();
            descriptor.SetProperty(new ApiDescriptionActionData());
            descriptor.DisplayName = actionFixtureName;

            descriptor.ActionConstraints = new List<IActionConstraintMetadata>
            {
                new HttpMethodConstraint(new[] { httpMethod })
            };
            descriptor.AttributeRouteInfo = new AttributeRouteInfo { Template = routeTemplate };

            descriptor.MethodInfo = typeof(ActionFixtures).GetMethod(actionFixtureName);
            if (descriptor.MethodInfo == null)
                throw new InvalidOperationException(
                    string.Format("{0} is not declared in ActionFixtures", actionFixtureName));

            descriptor.Parameters = descriptor.MethodInfo.GetParameters()
                .Select(paramInfo => new ParameterDescriptor
                    {
                        Name = paramInfo.Name,
                        ParameterType = paramInfo.ParameterType,
                        BinderMetadata = paramInfo.GetCustomAttributes(false)
                            .OfType<IBinderMetadata>()
                            .FirstOrDefault()
                })
                .ToList();

            return descriptor;
        }

        private IReadOnlyList<ApiDescription> GetApiDescriptions()
        {
            var context = new ApiDescriptionProviderContext(_actionDescriptors);

            var formattersProvider = new Mock<IOutputFormattersProvider>();
            formattersProvider.Setup(i => i.OutputFormatters).Returns(new[] { new JsonOutputFormatter() });

            var constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(i => i.ResolveConstraint("int")).Returns(new IntRouteConstraint());

            var provider = new DefaultApiDescriptionProvider(
                formattersProvider.Object,
                constraintResolver.Object,
                CreateModelMetadataProvider()
            );

            provider.Invoke(context, () => { });
            return new ReadOnlyCollection<ApiDescription>(context.Results);
        }

        private IModelMetadataProvider CreateModelMetadataProvider()
        {
            return new DataAnnotationsModelMetadataProvider();
            //var metadataDetailsProvider = new DefaultCompositeMetadataDetailsProvider(
            //    new IMetadataDetailsProvider[]
            //    {
            //        new DefaultBindingMetadataProvider(),
            //        new DataAnnotationsMetadataDetailsProvider()
            //    }
            //);
            //return new DefaultModelMetadataProvider(metadataDetailsProvider);
        }
    }
}