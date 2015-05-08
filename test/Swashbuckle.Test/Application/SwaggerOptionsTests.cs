using Microsoft.AspNet.Http;
using Xunit;
using Moq;
using Swashbuckle.Application;
using Microsoft.AspNet.Http.Core.Collections;

namespace Swashbuckle.Application
{
    public class SwaggerOptionsTests
    {
        [Theory]
        [InlineData("http", "tempuri.org:1234", "/api", "http://tempuri.org:1234/api")]
        [InlineData("http", "tempuri.org", null, "http://tempuri.org:80")]
        public void DefaultRooteUrlResolver_InfersRouteUrlFromRequest(
            string requestScheme,
            string requestHost,
            string requestPathBase,
            string expectedRootUrl)
        {
            var request = new Mock<HttpRequest>();
            request.Setup(req => req.Scheme).Returns(requestScheme);
            request.Setup(req => req.Host).Returns(new HostString(requestHost));
            request.Setup(req => req.PathBase).Returns(new PathString(requestPathBase));
            request.Setup(req => req.Headers).Returns(new HeaderDictionary());

            var rootUrl = SwaggerOptions.DefaultRootUrlResolver(request.Object);

            Assert.Equal(expectedRootUrl, rootUrl);
        }

        [Fact]
        public void DefaultRooteUrlResolver_InfersRouteUrlFromXForwardedHeaders_IfPresent()
        {
            var request = new Mock<HttpRequest>();
            request.Setup(req => req.Scheme).Returns("http");
            request.Setup(req => req.Host).Returns(new HostString("tempuri.org:1234"));
            request.Setup(req => req.Headers).Returns(new HeaderDictionary()
            {
                { "X-Forwarded-Proto", new[] { "https" } },
                { "X-Forwarded-Host", new[] { "acmecorp.org" } },
                { "X-Forwarded-Port", new[] { "5678" } }
            });

            var rootUrl = SwaggerOptions.DefaultRootUrlResolver(request.Object);

            Assert.Equal("https://acmecorp.org:5678", rootUrl);
        }
    }
}