using IdentityModel.OidcClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace orange_thallium.classes
{
  /// <summary>
  /// Helper class for managing OpenID connect
  /// </summary>
  class OidcManager
  {
    /// <summary>
    /// OIDC browser singleton
    /// </summary>
    private static Browser browser = new Browser();

    /// <summary>
    /// Attempt to authenticate the user 
    /// </summary>
    /// <returns>Whether or not the user successfully authenticated</returns>
    public static async Task<bool> Authenticate()
    {
      //Initialize the OIDC client
      OidcClientOptions clientOptions = new OidcClientOptions
      {
        Authority = StateManager.state.oidcSettings.endpoint,
        ClientId = StateManager.state.oidcSettings.clientID,
        RedirectUri = "http://localhost/callback",
        Scope = StateManager.state.oidcSettings.scope,
        Browser = browser
      };

      //Add client secret if enabled and provided
      if (StateManager.state.oidcSettings.clientSecretEnabled && StateManager.state.oidcSettings.clientSecret != null)
      {
        StateManager.state.oidcSettings.clientSecret.RetrievePassword();
        clientOptions.ClientSecret = StateManager.state.oidcSettings.clientSecret.Password;
      }

      OidcClient client = new OidcClient(clientOptions);

      //Login as the configured OIDC client
      LoginResult loginResult = await client.LoginAsync();

      //If successful, initialize the API client
      if (!loginResult.IsError)
      {
        //Instantiate an HTTP client
        HttpClient hookableClient = new HttpClient(new ApiMessageHandler());

        //Add the bearer token (OIDC ID Token)
        hookableClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.IdentityToken);

        //Instantiate the API client
        StateManager.state.client = new ApiClient(StateManager.state.generalSettings.apiAddress, hookableClient);

        //Update the authentication status
        StateManager.state.authenticated = true;

        return true;
      }

      return false;
    }
  }
}
