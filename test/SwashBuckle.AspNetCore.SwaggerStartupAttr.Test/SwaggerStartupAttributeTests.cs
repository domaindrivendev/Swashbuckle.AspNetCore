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
        private const string _solutionName = "Swashbuckle.AspNetCore";
        private const string _projectName = "CliMultipleStartupsExample";
        private const string _swaggerDirName = "bin\\SwaggerDir";
        private readonly string _projectFolder;
        private readonly string _swaggerDir;

        public SwaggerStartupAttributeTests()
        {
            string pwd = Path.GetFullPath(Directory.GetCurrentDirectory());

            string solutionDir = Path.Combine(
                pwd.Substring(0, pwd.IndexOf($"\\{_solutionName}")),
                $"{_solutionName}\\");

            _projectFolder = Path.Combine(solutionDir, $"test\\WebSites\\{_projectName}");

            _swaggerDir = Path.Combine(_projectFolder, _swaggerDirName);
        }

        [Fact]
        public void FileGeneration()
        {
            string publishCommand = $"dotnet publish {_projectFolder}{_projectName}.csproj -c Release";
            publishCommand.Run();

            Assert.True(Directory.Exists(_swaggerDir));

            Assembly assembly = Assembly.GetAssembly(typeof(CliMultipleStartupsExample.PrivateApiController));

            ICollection<Type> startupClasses = assembly.GetClassesWithSwaggerStartupAttribute().ToList();
            foreach (Type startupClass in startupClasses)
            {
                string expectedFileName = startupClass.GetSwaggerStartupAttribute().OpenApiFileName;
                string filePath = Path.Combine(_swaggerDir, expectedFileName);
                Assert.True(File.Exists(filePath));

                string parsedFile = null;
                using (StreamReader reader = new StreamReader(filePath))
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
