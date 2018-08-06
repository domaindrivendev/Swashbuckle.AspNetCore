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
                string filePath = Path.Combine(SwaggerDir, expectedFileName);
                Assert.True(File.Exists(filePath));

                StreamReader reader = new StreamReader(filePath);
                string parsedFile = null;
                using (reader)
                {
                    parsedFile = reader.ReadToEnd();
                }

                Assert.NotNull(parsedFile);

                if (startupClass.Name == nameof(CliStartupAttributeExample.Startups.PublicStartup))
                {
                    Assert.Contains("Public test API", parsedFile);

                    Assert.DoesNotContain("privateAPI", parsedFile);
                    Assert.DoesNotContain("post", parsedFile);
                    Assert.DoesNotContain("PrivateAPIPost", parsedFile);
                    Assert.DoesNotContain("delete", parsedFile);
                    Assert.DoesNotContain("PrivateAPIDelete", parsedFile);

                    Assert.Contains("publicAPI", parsedFile);
                    Assert.Contains("get", parsedFile);
                    Assert.Contains("PublicAPIByIdGet", parsedFile);
                }

                if (startupClass.Name == nameof(CliStartupAttributeExample.Startups.PrivateStartup))
                {
                    Assert.Contains("Private test API", parsedFile);

                    Assert.Contains("privateAPI", parsedFile);
                    Assert.Contains("post", parsedFile);
                    Assert.Contains("PrivateAPIPost", parsedFile);
                    Assert.Contains("delete", parsedFile);
                    Assert.Contains("PrivateAPIDelete", parsedFile);

                    Assert.DoesNotContain("publicAPI", parsedFile);
                    Assert.DoesNotContain("get", parsedFile);
                    Assert.DoesNotContain("PublicAPIByIdGet", parsedFile);
                }
            }
        }
    }
}
