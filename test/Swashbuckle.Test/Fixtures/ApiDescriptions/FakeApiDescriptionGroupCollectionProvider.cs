using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Constraints;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.ActionConstraints;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.AspNet.Mvc.Controllers;
using Microsoft.AspNet.Mvc.Formatters;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Metadata;
using Moq;

namespace Swashbuckle.Swagger.Fixtures.ApiDescriptions
{
    public class FakeApiDescriptionGroupCollectionProvider : IApiDescriptionGroupCollectionProvider
    {
        private readonly List<ControllerActionDescriptor> _actionDescriptors;

        public FakeApiDescriptionGroupCollectionProvider()
        {
            _actionDescriptors = new List<ControllerActionDescriptor>();
        }

        public FakeApiDescriptionGroupCollectionProvider Add(
            string httpMethod,
            string routeTemplate,
            string actionFixtureName,
            string controllerFixtureName = "NotAnnotated"
        )
        {
            _actionDescriptors.Add(
                CreateActionDescriptor(httpMethod, routeTemplate, actionFixtureName, controllerFixtureName));
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
            string httpMethod,
            string routeTemplate,
            string actionFixtureName,
            string controllerFixtureName
        )
        {
            var descriptor = new ControllerActionDescriptor();
            descriptor.SetProperty(new ApiDescriptionActionData());
            descriptor.DisplayName = actionFixtureName;

            descriptor.ActionConstraints = new List<IActionConstraintMetadata>();
            if (httpMethod != null)
                descriptor.ActionConstraints.Add(new HttpMethodConstraint(new[] { httpMethod }));

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
                        BindingInfo = BindingInfo.GetBindingInfo(paramInfo.GetCustomAttributes(false))
                    })
                .ToList();

            var controllerType = typeof(ControllerFixtures).GetNestedType(controllerFixtureName);
            if (controllerType == null)
                throw new InvalidOperationException(
                    string.Format("{0} is not declared in ControllerFixtures", controllerFixtureName));
            descriptor.ControllerTypeInfo = controllerType.GetTypeInfo();

            return descriptor;
        }

        private IReadOnlyList<ApiDescription> GetApiDescriptions()
        {
            var context = new ApiDescriptionProviderContext(_actionDescriptors);

            var options = new MvcOptions();
            options.OutputFormatters.Add(new JsonOutputFormatter());

            var optionsAccessor = new Mock<IOptions<MvcOptions>>();
            optionsAccessor.Setup(o => o.Value).Returns(options);

            var constraintResolver = new Mock<IInlineConstraintResolver>();
            constraintResolver.Setup(i => i.ResolveConstraint("int")).Returns(new IntRouteConstraint());

            var provider = new DefaultApiDescriptionProvider(
                optionsAccessor.Object,
                constraintResolver.Object,
                CreateModelMetadataProvider()
            );

            provider.OnProvidersExecuting(context);
            provider.OnProvidersExecuted(context);
            return new ReadOnlyCollection<ApiDescription>(context.Results);
        }

        private IModelMetadataProvider CreateModelMetadataProvider()
        {
            var metadataDetailsProvider = new DefaultCompositeMetadataDetailsProvider(
                new IMetadataDetailsProvider[]
                {
                    new DefaultBindingMetadataProvider(new ModelBindingMessageProvider
                    {
                        MissingBindRequiredValueAccessor = name => $"A value for the '{ name }' property was not provided.",
                        MissingKeyOrValueAccessor = () => $"A value is required.",
                        ValueMustNotBeNullAccessor = value => $"The value '{ value }' is invalid.",
                    }),
                    new DataAnnotationsMetadataProvider()
                }
            );
            return new DefaultModelMetadataProvider(metadataDetailsProvider);
        }
    }
}