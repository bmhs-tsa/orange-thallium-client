using Global;
using System;
using Windows.ApplicationModel;
using Windows.Storage;

namespace orange_thallium.classes
{
  /// <summary>
  /// Helper class for managing app launching
  /// </summary>
  class LaunchManager
  {
    /// <summary>
    /// Encrypt and write a credential field to be passed to the full-trust launcher
    /// </summary>
    /// <param name="key">The globally recognized credential field</param>
    /// <param name="field">The credential field</param>
    private static void WriteCredentialField(string key, string field)
    {
      //Encrypt the field
      string encryptedField = Globals.Encrypt(field);

      //Write the encrypted field to the disk
      ApplicationData.Current.LocalSettings.Values[key] = encryptedField;
    }

    /// <summary>
    /// Launch an app with the given username and password
    /// </summary>
    /// <param name="appID">The globally recognized app ID</param>
    /// <param name="username">The platform username</param>
    /// <param name="password">The platform password</param>
    /// <param name="mfaCode">The platform MFA code</param>
    public static async void Launch(string appID, string username, string password, string? mfaCode)
    {
      //Ensure the app is supported
      if (!Globals.apps.Exists(app => app.ID == appID))
      {
        throw new Exception(string.Format("Invalid app {0}", appID));
      }

      //Write the app ID
      ApplicationData.Current.LocalSettings.Values[Globals.appKey] = appID;

      //Write the credentials
      WriteCredentialField(Globals.usernameKey, username);
      WriteCredentialField(Globals.passwordKey, password);

      //If the MFA code was provided, write it too
      if (mfaCode != null)
      {
        WriteCredentialField(Globals.mfaCodeKey, mfaCode);
      }

      //Start the launcher
      await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
    }
  }
}
