using Microsoft.AspNetCore.Mvc;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures
{
    public class ApiInfo
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public ApiVersion ApiVersion { get; set; }

        public ApiInfo(string title, string description, ApiVersion apiVersion)
        {
            Title = title;
            Description = description;
            ApiVersion = apiVersion;
        }
    }
}
