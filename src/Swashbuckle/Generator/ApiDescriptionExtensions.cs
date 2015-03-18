using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public static class ApiDescriptionExtensions
    {
        //    public static string FriendlyId(this ApiDescription apiDescription)
        //    {
        //        return String.Format("{0}_{1}",
        //            apiDescription.ActionDescriptor.ControllerDescriptor.ControllerName,
        //            apiDescription.ActionDescriptor.ActionName);
        //    }

        //public static IEnumerable<string> Consumes(this ApiDescription apiDescription)
        //{
        //    return apiDescription.SupportedRequestBodyFormatters
        //        .SelectMany(formatter => formatter.SupportedMediaTypes.Select(mediaType => mediaType.MediaType))
        //        .Distinct();
        //}

        public static IEnumerable<string> Produces(this ApiDescription apiDescription)
        {
            return apiDescription.SupportedResponseFormats
                .Select(format => format.MediaType.MediaType)
                .Distinct();
        }

        public static string RelativePathSansQueryString(this ApiDescription apiDescription)
        {
            return apiDescription.RelativePath.Split('?').First();
        }

        //    public static Type ResponseType(this ApiDescription apiDesc)
        //    {
        //        // HACK: The ResponseDescription property was introduced in WebApi 5.0 but Swashbuckle supports >= 4.0.
        //        // The reflection hack below provides support for the ResponseType attribute if the application is running
        //        // against a version of WebApi that supports it.
        //        var apiDescType = typeof(ApiDescription);

        //        var responseDescPropInfo = apiDescType.GetProperty("ResponseDescription");
        //        if (responseDescPropInfo != null)
        //        {
        //            var responseDesc = responseDescPropInfo.GetValue(apiDesc, null);
        //            if (responseDesc != null)
        //            {
        //                var responseDescType = responseDesc.GetType();

        //                var responseTypePropInfo = responseDescType.GetProperty("ResponseType");
        //                if (responseTypePropInfo != null)
        //                {
        //                    var responseType = responseTypePropInfo.GetValue(responseDesc, null);
        //                    if (responseType != null)
        //                        return (Type)responseType;
        //                }
        //            }
        //        }

        //        // Otherwise, it defaults to the declared response type
        //        return apiDesc.ActionDescriptor.ReturnType;
        //    }

        //    public static bool IsObsolete(this ApiDescription apiDescription)
        //    {
        //        return apiDescription.ActionDescriptor.GetCustomAttributes<ObsoleteAttribute>().Any();
        //    }
    }
}