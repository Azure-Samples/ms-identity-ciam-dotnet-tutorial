namespace sign_in_dotnet_wpf
{
    /// <summary>
    /// App settings, populated from appsettings.json file
    /// </summary>
    public class AzureAdConfig
    {
        /// <summary>
        /// Gets or sets the Azure AD authority.
        /// </summary>
        /// <value>
        /// The Azure AD authority URL.
        /// </value>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the client Id (App Id) from the app registration in the Azure AD portal.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public string ClientId { get; set; }
    }
}