using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Mvc;

namespace OAuth2Integration.AuthServer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ConsentController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public ConsentController(
            IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [HttpGet("consent")]
        public async Task<IActionResult> Consent(string returnUrl)
        {
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);

            var viewModel = new ConsentViewModel
            {
                ReturnUrl = returnUrl,
                ClientName = request.Client.ClientName,
                ScopesRequested = request.ValidatedResources?.Resources?.ApiScopes ?? []
            };

            return View("/AuthServer/Views/Consent.cshtml", viewModel);
        }

        [HttpPost("consent")]
        public async Task<IActionResult> Consent([FromForm] ConsentViewModel viewModel)
        {
            var request = await _interaction.GetAuthorizationContextAsync(viewModel.ReturnUrl);

            ConsentResponse consentResponse;
            if (viewModel.ScopesConsented != null && viewModel.ScopesConsented.Any())
            {
                consentResponse = new ConsentResponse
                {
                    RememberConsent = true,
                    ScopesValuesConsented = viewModel.ScopesConsented.ToList()
                };
            }
            else
            {
                consentResponse = new ConsentResponse
                {
                    Error = AuthorizationError.AccessDenied
                };
            }

            await _interaction.GrantConsentAsync(request, consentResponse);

            return Redirect(viewModel.ReturnUrl);
        }
    }

    public class ConsentViewModel
    {
        public string ReturnUrl { get; set; }
        public string ClientName { get; set; }
        public IEnumerable<ApiScope> ScopesRequested { get; set; }
        public string[] ScopesConsented { get; set; }
    }
}
