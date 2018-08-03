using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

using Swashbuckle.AspNetCore.StartupAttribute;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    public class StartupAttributeTests
    {
        private const string ProjectName = "CliStartupAttributeExample";
        private const string ProjectFolder = "../../../../WebSites/" + ProjectName + "/";
        private const string SwaggerDirName = "bin/SwaggerDir";
        private const string SwaggerDir = ProjectFolder + SwaggerDirName;

        [Fact]
        public void FileGeneration()
        {
            string publishCommand = $"dotnet publish {ProjectFolder}{ProjectName}.csproj -c Release";
            publishCommand.Run();

            Assert.True(Directory.Exists(SwaggerDir));

            Assembly assembly = Assembly.GetAssembly(typeof(CliStartupAttributeExample.PrivateApiController));

            ICollection<Type> startupClasses = assembly.GetClassesWithStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                string expectedFileName = startupClass.GetStartupAttributeName() + ".swagger.json";
                Assert.True(File.Exists(Path.Combine(SwaggerDir, expectedFileName)));
            }
        }
    }
}
