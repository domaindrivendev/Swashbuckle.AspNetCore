using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Swashbuckle.AspNetCore.StartupAttribute;
using SwashBuckle.AspNetCore.StartupAttribute.Test.Startups;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    public class StartupAttributeHerlpersTests
    {
        [Fact]
        public void GetClassesWithStartupAttribute()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(StartupAttributeHerlpersTests));

            ICollection<Type> startupClasses = assembly.GetClassesWithStartupAttribute().ToList();
            Assert.Equal(2, startupClasses.Count);
            Assert.Contains(nameof(PublicStartup), startupClasses.Select(type => type.Name));
            Assert.Contains(nameof(PrivateStartup), startupClasses.Select(type => type.Name));
        }

        [Fact]
        public void GetStartupAttributeName()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(StartupAttributeHerlpersTests));

            ICollection<Type> startupClasses = assembly.GetClassesWithStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                if (nameof(PublicStartup) == startupClass.Name)
                {
                    Assert.Equal("PublicAPI", startupClass.GetStartupAttributeName());
                }
                if (nameof(PrivateStartup) == startupClass.Name)
                {
                    Assert.Equal("PrivateAPI", startupClass.GetStartupAttributeName());
                }
            }
        }
    }
}
