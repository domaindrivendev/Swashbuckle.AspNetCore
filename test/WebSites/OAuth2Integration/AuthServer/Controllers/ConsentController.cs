using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace OAuth2Integration.AuthServer.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class ConsentController(IIdentityServerInteractionService interaction) : Controller
{
    [HttpGet("consent")]
    public async Task<IActionResult> Consent(string returnUrl)
    {
        var request = await interaction.GetAuthorizationContextAsync(returnUrl);

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
        var request = await interaction.GetAuthorizationContextAsync(viewModel.ReturnUrl);

        ConsentResponse consentResponse;
        if (viewModel.ScopesConsented != null && viewModel.ScopesConsented.Length != 0)
        {
            consentResponse = new ConsentResponse
            {
                RememberConsent = true,
                ScopesValuesConsented = [.. viewModel.ScopesConsented],
            };
        }
        else
        {
            consentResponse = new ConsentResponse
            {
                Error = AuthorizationError.AccessDenied
            };
        }

        await interaction.GrantConsentAsync(request, consentResponse);

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
