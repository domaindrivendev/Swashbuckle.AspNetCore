using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using IdentityServer4;
using IdentityServer4.Test;

namespace OAuth2Integration.AuthServer.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly TestUserStore _userStore;

        public AccountController()
        {
            _userStore = new TestUserStore(Config.TestUsers());
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            var viewModel = new LoginViewModel { Username = "joebloggs", Password = "pass123", ReturnUrl = returnUrl };

            return View("/AuthServer/Views/Login.cshtml", viewModel);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm]LoginViewModel viewModel)
        {
            if (!_userStore.ValidateCredentials(viewModel.Username, viewModel.Password))
            {
                ModelState.AddModelError("", "Invalid username or password");
                viewModel.Password = string.Empty;
                return View("/AuthServer/Views/Login.cshtml", viewModel);
            }

            // Use an IdentityServer-compatible ClaimsPrincipal
            var principal = IdentityServerPrincipal.Create(viewModel.Username, viewModel.Username);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

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
