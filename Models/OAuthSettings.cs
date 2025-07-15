namespace ModelHierarchyApp.Models
{
    /// <summary>
    /// Represents the configuration settings required for OAuth authentication.
    /// </summary>
    public class OAuthSettings
    {
        /// <summary>
        /// Gets or sets the client ID for OAuth authentication.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret for OAuth authentication.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the redirect URI to which the user will be redirected after authentication.
        /// </summary>
        public string RedirectUri { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the token URL for exchanging the authorization code for an access token.
        /// </summary>
        public string TokenUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scope(s) required for the OAuth authentication.
        /// </summary>
        public string Scopes { get; set; } = string.Empty;
    }
}
