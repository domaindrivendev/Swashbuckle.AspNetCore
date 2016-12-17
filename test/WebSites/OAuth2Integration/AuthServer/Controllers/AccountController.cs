using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Services.InMemory;
using IdentityServer4;

namespace OAuth2Integration.AuthServer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly InMemoryUserLoginService _loginService;

        public AccountController(InMemoryUserLoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel { ReturnUrl = returnUrl };

            return View("/AuthServer/Views/Login.cshtml", viewModel);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm]LoginViewModel viewModel)
        {
            if (!_loginService.ValidateCredentials(viewModel.Username, viewModel.Password))
            {
                ModelState.AddModelError("", "Invalid username or password");
                viewModel.Password = string.Empty;
                return View("/AuthServer/Views/Login.cshtml", viewModel);
            }

            // Use an IdentityServer-compatible ClaimsPrincipal
            var principal = IdentityServerPrincipal.Create(viewModel.Username, viewModel.Username);
            await HttpContext.Authentication.SignInAsync("Cookies", principal);

            return Redirect(viewModel.ReturnUrl);
        }
    }

    public class LoginViewModel
    {
        public string ReturnUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
