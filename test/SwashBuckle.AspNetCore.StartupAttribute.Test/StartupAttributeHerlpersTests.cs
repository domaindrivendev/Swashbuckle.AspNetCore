using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

using Swashbuckle.AspNetCore.StartupAttribute;

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
            Assert.Contains(nameof(Startup2), startupClasses.Select(type => type.Name));
            Assert.Contains(nameof(Startup1), startupClasses.Select(type => type.Name));
        }

        [Fact]
        public void GetStartupAttributeName()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(StartupAttributeHerlpersTests));

            ICollection<Type> startupClasses = assembly.GetClassesWithStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                if (nameof(Startup2) == startupClass.Name)
                {
                    Assert.Equal("TestStartup2", startupClass.GetStartupAttributeName());
                }
                if (nameof(Startup1) == startupClass.Name)
                {
                    Assert.Equal("TestStartup1", startupClass.GetStartupAttributeName());
                }
            }
        }
    }
}
