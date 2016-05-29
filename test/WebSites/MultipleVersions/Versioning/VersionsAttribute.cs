using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System.Linq;

namespace MultipleVersions.Versioning
{
    public class VersionsAttribute : Attribute, IActionConstraintFactory
    {
        public VersionsAttribute(params string[] acceptedVersions)
        {
            AcceptedVersions = acceptedVersions;
        }

        public string[] AcceptedVersions { get; private set; }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return new VersionConstraint(AcceptedVersions);
        }
    }

    public class VersionConstraint : IActionConstraint
    {
        private readonly string[] _acceptedVersions;
        public VersionConstraint(string[] acceptedVersions)
        {
            Order = -1;
            _acceptedVersions = acceptedVersions;
        }

        public int Order { get; set; }

        public bool Accept(ActionConstraintContext context)
        {
            var versionValue = context.RouteContext.RouteData.Values["version"];
            if (versionValue == null) return false;

            return _acceptedVersions.Contains(versionValue.ToString());
        }
    }
}