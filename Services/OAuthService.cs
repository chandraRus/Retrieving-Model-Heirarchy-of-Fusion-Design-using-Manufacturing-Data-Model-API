using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ModelHierarchyApp.Models;

namespace ModelHierarchyApp.Services
{
    /// <summary>
    /// Handles OAuth authentication logic with Autodesk's API.
    /// </summary>
    public class OAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OAuthSettings _oauthSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The HTTP client factory for creating HTTP clients.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor for accessing session data.</param>
        /// <param name="oauthOptions">The OAuth settings options.</param>
        public OAuthService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IOptions<OAuthSettings> oauthOptions)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _oauthSettings = oauthOptions.Value;
        }

        /// <summary>
        /// Builds the Autodesk login URL for the 3-legged OAuth flow.
        /// </summary>
        /// <returns>The login URL to redirect the user for authentication.</returns>
        public string GetLoginUrl()
        {
            return $"https://developer.api.autodesk.com/authentication/v2/authorize" +
                   $"?response_type=code" +
                   $"&client_id={_oauthSettings.ClientId}" +
                   $"&redirect_uri={_oauthSettings.RedirectUri}" +
                   $"&scope={_oauthSettings.Scopes}";
        }

        /// <summary>
        /// Exchanges the authorization code for an access token and stores it in the session.
        /// </summary>
        /// <param name="code">The authorization code returned by Autodesk.</param>
        /// <returns>The access token as a string.</returns>
        public async Task<string> ExchangeCodeForTokenAsync(string code)
        {
            var client = _httpClientFactory.CreateClient();

            // Construct the POST request body for token exchange
            var content = new StringContent(
                $"client_id={_oauthSettings.ClientId}&" +
                $"client_secret={_oauthSettings.ClientSecret}&" +
                $"grant_type=authorization_code&" +
                $"code={code}&" +
                $"redirect_uri={_oauthSettings.RedirectUri}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );

            // Send the request to Autodesk token endpoint
            var response = await client.PostAsync(_oauthSettings.TokenUrl, content);
            response.EnsureSuccessStatusCode();

            // Parse the access token from the JSON response
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            // Store the token in session for future requests
            _httpContextAccessor.HttpContext.Session.SetString("access_token", token!);

            return token!;
        }

        /// <summary>
        /// Retrieves the access token stored in the current session.
        /// </summary>
        /// <returns>The access token if present; otherwise, null.</returns>
        public string? GetAccessTokenFromSession()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("access_token");
        }
    }
}
