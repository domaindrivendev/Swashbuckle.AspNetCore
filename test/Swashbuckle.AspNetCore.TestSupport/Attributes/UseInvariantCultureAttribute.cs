using System;
using System.Globalization;
using System.Reflection;

using Xunit.Sdk;

namespace Swashbuckle.AspNetCore.TestSupport
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class UseInvariantCultureAttribute : BeforeAfterTestAttribute
    {
        public override void Before(MethodInfo methodUnderTest)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        }
    }
}
