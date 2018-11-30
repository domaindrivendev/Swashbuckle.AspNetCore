using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiKeySecurity
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions> 
    {
        private readonly IEnumerable<string> ValidApiKeys = new[]
        {
            "test-system"
        };

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Query.ContainsKey("apiKey"))
                return Task.FromResult(AuthenticateResult.Fail("Missing apiKey"));

            var submittedApiKey = Request.Query["apiKey"].Last();

            if (!ValidApiKeys.Contains(submittedApiKey))
                return Task.FromResult(AuthenticateResult.Fail("Invalid apiKey"));

            var systemIdentity = new ClaimsIdentity(
                authenticationType: Scheme.Name,
                claims: new[] { new Claim(ClaimTypes.System, submittedApiKey) });

            var systemPrincipal = new ClaimsPrincipal(systemIdentity);

            var authenticationTicket = new AuthenticationTicket(
                authenticationScheme: Scheme.Name,
                principal: systemPrincipal);

            return Task.FromResult(AuthenticateResult.Success(authenticationTicket));
        }
    }
}
