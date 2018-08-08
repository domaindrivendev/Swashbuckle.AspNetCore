using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

using Swashbuckle.AspNetCore.SwaggerStartupAttr;

namespace SwashBuckle.AspNetCore.SwaggerStartupAttr.Test
{
    public class SwaggerStartupAttributeTests
    {
        private const string ProjectName = "CliMultipleStartupsExample";
        private const string ProjectFolder = "../../../../WebSites/" + ProjectName + "/";
        private const string SwaggerDirName = "bin/SwaggerDir";
        private const string SwaggerDir = ProjectFolder + SwaggerDirName;

        [Fact]
        public void FileGeneration()
        {
            string publishCommand = $"dotnet publish {ProjectFolder}{ProjectName}.csproj -c Debug";
            var x = publishCommand.Run();

            Assert.True(Directory.Exists(SwaggerDir));

            Assembly assembly = Assembly.GetAssembly(typeof(CliMultipleStartupsExample.PrivateApiController));

            ICollection<Type> startupClasses = assembly.GetClassesWithSwaggerStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                string expectedFileName = startupClass.GetSwaggerStartupAttribute().OpenApiFileName;
                string filePath = Path.Combine(SwaggerDir, expectedFileName);
                Assert.True(File.Exists(filePath));

                StreamReader reader = new StreamReader(filePath);
                string parsedFile = null;
                using (reader)
                {
                    parsedFile = reader.ReadToEnd();
                }

                Assert.NotNull(parsedFile);

                if (startupClass.Name == nameof(CliMultipleStartupsExample.Startups.PublicStartup))
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

                if (startupClass.Name == nameof(CliMultipleStartupsExample.Startups.PrivateStartup))
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
