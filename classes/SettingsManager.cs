using System;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.Storage;

namespace orange_thallium.classes
{
  /// <summary>
  /// General settings that don't relate to OIDC authentication
  /// </summary>
  class GeneralSettings
  {
    public string apiAddress { get; set; } = "https://example.com";
    public string accountID { get; set; } = "test@example.com";
  }

  /// <summary>
  /// OpenID Connect (OIDC) settings
  /// </summary>
  class OidcSettings
  {
    public string endpoint { get; set; } = "https://demo.identityserver.io";
    public string clientID { get; set; } = "interactive.public";
    public PasswordCredential? clientSecret { get; set; }
    public bool clientSecretEnabled { get; set; } = false;
    public string scope { get; set; } = "openid";
  }

  /// <summary>
  /// Helper class for managing settings
  /// </summary>
  class SettingsManager
  {
    public static string OidcClientSecretKey = "Orange Thallium OpenID Client Secret";
    public static string OidcClientSecretUsername = "openidClientSecret";

    /// <summary>
    /// Get a credential
    /// </summary>
    /// <param name="resource">The Credential Manager resource name</param>
    /// <returns>The desired password credential or null if it doesn't exist</returns>
    private static PasswordCredential? GetCredential(string resource)
    {
      //Attempt to retrieve the credential
      try
      {
        IReadOnlyList<PasswordCredential> credentials = StateManager.state.vault.FindAllByResource(resource);

        //Return the first credential
        return credentials[0];
      }
      catch (Exception exception)
      {
        //Don't throw the error if the credential doesn't exist
        if (exception.HResult != -2147023728)
        {
          throw;
        }
        else
        {
          return null;
        }
      }
    }

    /// <summary>
    /// Insert or update a credential
    /// </summary>
    /// <param name="credential">The credential to insert/update</param>
    private static void UpsertCredential(PasswordCredential credential)
    {
      //Delete the credential if it already exists
      DeleteCredential(OidcClientSecretKey);

      //Add a new credential
      StateManager.state.vault.Add(credential);
    }

    /// <summary>
    /// Delete a credential
    /// </summary>
    /// <param name="resource">The credential to delete</param>
    private static void DeleteCredential(string resource)
    {
      //Get the credential
      PasswordCredential? credential = GetCredential(resource);

      //If a credential already exists, remove it
      if (credential != null)
      {
        StateManager.state.vault.Remove(credential);
      }
    }

    /// <summary>
    /// Load all settings from the local settings or use default values if missing
    /// 
    /// This will update the global state instead of returning anything
    /// </summary>
    public static void LoadSettings()
    {
      //Load settings
      ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

      if (localSettings.Values.ContainsKey("generalSettings"))
      {
        ApplicationDataCompositeValue generalSettings = localSettings.Values["generalSettings"] as ApplicationDataCompositeValue;
        StateManager.state.generalSettings.apiAddress = generalSettings["apiAddress"] as string;
        StateManager.state.generalSettings.accountID = generalSettings["accountID"] as string;
      }
      else
      {
        StateManager.state.generalSettings = new GeneralSettings();
      }

      if (localSettings.Values.ContainsKey("oidcSettings"))
      {
        ApplicationDataCompositeValue oidcSettings = localSettings.Values["oidcSettings"] as ApplicationDataCompositeValue;
        StateManager.state.oidcSettings.endpoint = oidcSettings["endpoint"] as string;
        StateManager.state.oidcSettings.clientID = oidcSettings["clientID"] as string;
        StateManager.state.oidcSettings.clientSecretEnabled = Convert.ToBoolean(oidcSettings["clientSecretEnabled"]);
        StateManager.state.oidcSettings.scope = oidcSettings["scope"] as string;
      }
      else
      {
        StateManager.state.oidcSettings = new OidcSettings();
      }

      //Load credentials
      StateManager.state.oidcSettings.clientSecret = GetCredential(OidcClientSecretKey);
    }

    /// <summary>
    /// Save all current global settings to the local settings
    /// </summary>
    public static void SaveSettings()
    {
      //Save settings
      ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

      ApplicationDataCompositeValue generalSettings = new ApplicationDataCompositeValue();
      generalSettings["apiAddress"] = StateManager.state.generalSettings.apiAddress;
      generalSettings["accountID"] = StateManager.state.generalSettings.accountID;
      localSettings.Values["generalSettings"] = generalSettings;

      ApplicationDataCompositeValue oidcSettings = new ApplicationDataCompositeValue();
      oidcSettings["endpoint"] = StateManager.state.oidcSettings.endpoint;
      oidcSettings["clientID"] = StateManager.state.oidcSettings.clientID;
      oidcSettings["clientSecretEnabled"] = StateManager.state.oidcSettings.clientSecretEnabled;
      oidcSettings["scope"] = StateManager.state.oidcSettings.scope;
      localSettings.Values["oidcSettings"] = oidcSettings;

      //If the OIDC client secret is enabled, save it
      if (StateManager.state.oidcSettings.clientSecretEnabled)
      {
        UpsertCredential(StateManager.state.oidcSettings.clientSecret);
      }
      //Otherwise delete the OIDC client secret (Improve security)
      else
      {
        DeleteCredential(OidcClientSecretKey);
      }
    }

    /// <summary>
    /// Reset all settings
    /// </summary>
    public static void ResetSettings()
    {
      StateManager.state.generalSettings = new GeneralSettings();
      StateManager.state.oidcSettings = new OidcSettings();
    }
  }
}
