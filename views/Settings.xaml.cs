using orange_thallium.classes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace orange_thallium.views
{
  struct JsonSettings
  {
    public string version;
    public GeneralSettings generalSettings;
    public OidcSettings oidcSettings;
  }

  public sealed partial class Settings : Page
  {
    private Version version;
    private GeneralSettings generalSettings;
    private OidcSettings oidcSettings;

    public Settings()
    {
      this.InitializeComponent();

      //Load settings
      this.loadSettings();

      //Enable the secret input if enabled and vice-versa
      this.oidcClientSecret.IsEnabled = this.oidcClientSecretEnabled.IsOn;

      //Update version
      this.version = new Version(Assembly.GetExecutingAssembly().GetName().Version.ToString());

      //Update version text
      this.verionText.Text = string.Format("Version {0}", this.version.ToString());
    }

    /// <summary>
    /// Reset local and global settings
    /// </summary>
    private void resetSettings()
    {
      //Reset global settings
      SettingsManager.ResetSettings();

      //Reload local settings
      this.loadSettings();

      //Force update the UI
      Bindings.Update();

      //Save the settings
      this.saveSettings();
    }

    /// <summary>
    /// Save local settings to global settings
    /// </summary>
    private void saveSettings()
    {
      //Add 'openid' scope if not present
      if (!Array.Exists(this.oidcSettings.scope.Split(' '), scope => scope == "openid"))
      {
        this.oidcSettings.scope += " openid";
      }

      //If secret is newly enabled but empty (Not just cleared), disable
      if (!StateManager.state.oidcSettings.clientSecretEnabled &&
        this.oidcSettings.clientSecretEnabled &&
        this.oidcClientSecret.Password.Length == 0)
      {
        this.oidcSettings.clientSecretEnabled = false;

        //Force update the UI
        Bindings.Update();
      }

      //If secret is enabled and not empty, update
      if (this.oidcSettings.clientSecretEnabled && this.oidcClientSecret.Password.Length > 0)
      {
        oidcSettings.clientSecret = new PasswordCredential(SettingsManager.OidcClientSecretKey, SettingsManager.OidcClientSecretUsername, this.oidcClientSecret.Password);
      }

      //Clear secret input
      this.oidcClientSecret.Password = "";

      //Update global settings with local settings
      StateManager.state.generalSettings = this.generalSettings;
      StateManager.state.oidcSettings = this.oidcSettings;

      //Save settings
      SettingsManager.SaveSettings();
    }

    /// <summary>
    /// Load local settings from global settings
    /// </summary>
    private void loadSettings()
    {
      //Initialize local settings with global settings
      generalSettings = StateManager.state.generalSettings;
      oidcSettings = StateManager.state.oidcSettings;
    }

    /// <summary>
    /// Export settings to a JSON file
    /// </summary>
    private async void exportSettings()
    {
      //Instantiate a file picker
      FileSavePicker picker = new FileSavePicker();
      picker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
      picker.SuggestedFileName = "Orange Thallium Settings";
      picker.SuggestedStartLocation = PickerLocationId.Downloads;

      //Open the picker
      StorageFile file = await picker.PickSaveFileAsync();

      if (file != null)
      {
        //Construct a settings struct without secrets
        JsonSettings settings = new JsonSettings
        {
          version = this.version.ToString(),
          generalSettings = this.generalSettings,
          oidcSettings = this.oidcSettings
        };

        //Serialize the settings
        string raw = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
          Converters =
          {
            new PasswordCredentialConverter()
          },
          IncludeFields = true,
          WriteIndented = true
        });

        //Save the file
        await FileIO.WriteTextAsync(file, raw);
      }
    }

    /// <summary>
    /// Import settings from a JSON file
    /// </summary>
    private async void importSettings()
    {
      //Instantiate a file picker
      FileOpenPicker picker = new FileOpenPicker();
      picker.FileTypeFilter.Add(".json");
      picker.SuggestedStartLocation = PickerLocationId.Downloads;

      //Open the picker
      StorageFile file = await picker.PickSingleFileAsync();

      if (file != null)
      {
        //Read the file
        string raw = await FileIO.ReadTextAsync(file);

        //Deserialize the settings
        JsonSettings settings = JsonSerializer.Deserialize<JsonSettings>(raw, new JsonSerializerOptions
        {
          Converters =
          {
            new PasswordCredentialConverter()
          },
          IncludeFields = true
        });

        //Parse the setting version
        Version settingVersion = new Version(settings.version);

        //Ensure major and minor version matches
        if (settingVersion.Major == this.version.Major && settingVersion.Minor == this.version.Minor)
        {
          //Update local settings
          this.generalSettings = settings.generalSettings;
          this.oidcSettings = settings.oidcSettings;

          //Force update the UI
          Bindings.Update();

          //Save settings
          this.saveSettings();
        }
        //Otherwise display an error
        else
        {
          //Prompt the user
          ContentDialog dialog = new ContentDialog
          {
            Title = "Mismatched Config Version",
            Content = string.Format("The config file is incompatible with this version of Orange Thallium! (Expected {0}.{1}.X.X but received {2})", this.version.Major, this.version.Minor, settings.version),
            CloseButtonText = "Close"
          };

          await dialog.ShowAsync();
        }
      }
    }

    private async void reset_Click(object sender, RoutedEventArgs e)
    {
      //Prompt the user for confirmation
      ContentDialog dialog = new ContentDialog
      {
        Title = "Reset All Settings",
        Content = "Are you sure you want to reset all settings?",
        PrimaryButtonText = "Reset",
        SecondaryButtonText = "Don't Reset"
      };

      ContentDialogResult result = await dialog.ShowAsync();

      //Reset settings
      if (result == ContentDialogResult.Primary)
      {
        this.resetSettings();
      }
    }

    private void oidcClientSecret_PasswordChanged(object sender, RoutedEventArgs e)
    {
      //Get the raw secret
      string rawSecret = this.oidcClientSecret.Password;

      //Update the label
      this.oidcClientSecretLabel.Text = rawSecret.Length == 0 ? "Client Secret (Unchanged)" : "Client Secret";
    }

    private void oidcClientSecretEnabled_Toggled(object sender, RoutedEventArgs e)
    {
      //Enable the secret input if the secret is enabled and vice-versa
      this.oidcClientSecret.IsEnabled = this.oidcClientSecretEnabled.IsOn;
    }

    private void save_Click(object sender, RoutedEventArgs e)
    {
      //Save settings
      this.saveSettings();
    }

    private void cancel_Click(object sender, RoutedEventArgs e)
    {
      //Load settings
      this.loadSettings();
    }

    private void export_Click(object sender, RoutedEventArgs e)
    {
      //Export settings
      this.exportSettings();
    }

    private void import_Click(object sender, RoutedEventArgs e)
    {
      //Import settings
      this.importSettings();
    }
  }
}
