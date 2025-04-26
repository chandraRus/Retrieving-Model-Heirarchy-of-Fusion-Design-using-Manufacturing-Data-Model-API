using Microsoft.AspNetCore.Mvc;
using ModelHierarchyApp.Services;

namespace ModelHierarchyApp.Controllers
{
    /// <summary>
    /// Handles the OAuth authentication flow, including login and callback handling.
    /// </summary>
    public class OAuthController : Controller
    {
        // Private readonly field for OAuthService used in the controller
        private readonly OAuthService _oauthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthController"/> class.
        /// </summary>
        public OAuthController(OAuthService oauthService)
        {
            _oauthService = oauthService; // Initialize OAuthService
        }

        /// <summary>
        /// Action method to initiate the OAuth login process.
        /// </summary>
        /// <returns>Redirects to the OAuth login URL.</returns>
        [HttpGet]
        public IActionResult Login()
        {
            // Get the OAuth login URL from the OAuthService
            var loginUrl = _oauthService.GetLoginUrl();
            // Redirect the user to the OAuth login page
            return Redirect(loginUrl);
        }

        /// <summary>
        /// Callback action method that handles the OAuth response after the user authorizes the application.
        /// </summary>
        /// <param name="code">The authorization code returned by OAuth.</param>
        /// <returns>Redirects to the ModelHierarchy index action upon successful token exchange.</returns>
        [HttpGet("/callback/oauth")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            try
            {
                // Exchange the authorization code for an access token
                var token = await _oauthService.ExchangeCodeForTokenAsync(code);

                // Redirect to the ModelHierarchy index action to load the model data
                return RedirectToAction("Index", "ModelHierarchy");
            }
            catch (Exception ex)
            {
                // If an error occurs during the token exchange, display an error message
                return Content("OAuth error: " + ex.Message);
            }
        }
    }
}
