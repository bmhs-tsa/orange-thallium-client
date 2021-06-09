using orange_thallium.classes;
using Windows.Security.Credentials;

namespace orange_thallium
{
  /// <summary>
  /// The application state
  /// </summary>
  class State
  {
    public bool authenticated = false;
    public ApiClient client;
    public PasswordVault vault = new PasswordVault();
    public GeneralSettings generalSettings = new GeneralSettings();
    public OidcSettings oidcSettings = new OidcSettings();
  }

  class StateManager
  {
    /// <summary>
    /// The application state singleton. This should be used as the source of truth for any inter-page state
    /// </summary>
    public static State state = new State();
  }
}
