using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

using Swashbuckle.AspNetCore.SwaggerStartupAttr;

namespace SwashBuckle.AspNetCore.SwaggerStartupAttr.Test
{
    public class SwaggerStartupAttributeHerlpersTests
    {
        [Fact]
        public void GetClassesWithSwaggerStartupAttribute()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SwaggerStartupAttributeHerlpersTests));

            ICollection<Type> startupClasses = assembly.GetClassesWithSwaggerStartupAttribute().ToList();
            Assert.Equal(2, startupClasses.Count);
            Assert.Contains(nameof(Startup2), startupClasses.Select(type => type.Name));
            Assert.Contains(nameof(Startup1), startupClasses.Select(type => type.Name));
        }

        [Fact]
        public void GetSwaggerStartupAttribute()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(SwaggerStartupAttributeHerlpersTests));

            ICollection<Type> startupClasses = assembly.GetClassesWithSwaggerStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                if (nameof(Startup1) == startupClass.Name)
                {
                    Assert.Equal("TestStartup1.swagger.json", startupClass.GetSwaggerStartupAttribute().OpenApiFileName);
                    Assert.Equal("Client1", startupClass.GetSwaggerStartupAttribute().ClientClassName);
                    Assert.Equal("TestNamespace", startupClass.GetSwaggerStartupAttribute().ClientNamespace);
                }
                if (nameof(Startup2) == startupClass.Name)
                {
                    Assert.Equal("TestStartup2.swagger.json", startupClass.GetSwaggerStartupAttribute().OpenApiFileName);
                    Assert.Equal("Client2", startupClass.GetSwaggerStartupAttribute().ClientClassName);
                    Assert.Equal("TestNamespace", startupClass.GetSwaggerStartupAttribute().ClientNamespace);
                }
            }
        }
    }
}
