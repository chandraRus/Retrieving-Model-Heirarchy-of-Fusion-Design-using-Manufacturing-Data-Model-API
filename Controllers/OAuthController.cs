using Microsoft.AspNetCore.Mvc;
using ModelHierarchyApp.Services;

namespace ModelHierarchyApp.Controllers
{
    /// <summary>
    /// Handles the OAuth authentication flow, including login and callback handling.
    /// </summary>
    public class OAuthController : Controller
    {
        /// <summary>
        /// The OAuth service used for authentication logic.
        /// </summary>
        private readonly OAuthService _oauthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthController"/> class.
        /// </summary>
        /// <param name="oauthService">The OAuth service to use for authentication.</param>
        public OAuthController(OAuthService oauthService)
        {
            _oauthService = oauthService;
        }

        /// <summary>
        /// Initiates the OAuth login process by redirecting the user to the Autodesk login page.
        /// </summary>
        /// <returns>A redirect to the OAuth login URL.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            var loginUrl = _oauthService.GetLoginUrl();
            return Redirect(loginUrl);
        }

        /// <summary>
        /// Handles the OAuth callback after the user authorizes the application.
        /// Exchanges the authorization code for an access token and redirects to the model hierarchy index.
        /// </summary>
        /// <param name="code">The authorization code returned by OAuth.</param>
        /// <returns>
        /// Redirects to the ModelHierarchy index action upon successful token exchange,
        /// or displays an error message if the exchange fails.
        /// </returns>
        [HttpGet("/callback/oauth")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            try
            {
                var token = await _oauthService.ExchangeCodeForTokenAsync(code);
                return RedirectToAction("Index", "ModelHierarchy");
            }
            catch (Exception ex)
            {
                return Content("OAuth error: " + ex.Message);
            }
        }
    }
}
