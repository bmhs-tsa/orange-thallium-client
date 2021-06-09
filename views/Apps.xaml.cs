using FuzzySharp;
using Global;
using orange_thallium.classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace orange_thallium.views
{
  public sealed partial class Apps : Page
  {
    private static int threshold = 70;
    private List<Global.App> availableApps;
    private ObservableCollection<Global.App> visibleApps;

    public Apps()
    {
      this.InitializeComponent();

      //Initialize apps
      this.initializeApps();
    }

    /// <summary>
    /// Initialize both visible and viable apps
    /// </summary>
    private async void initializeApps()
    {
      //Get enabled platforms IDs
      ICollection<string> enabledPlatformIDs = await StateManager.state.client.GetAllPlatformsAsync();

      //Union with supported platforms
      this.availableApps = Globals.apps.FindAll(app => enabledPlatformIDs.Contains(app.PlatformID));

      //Update visible apps
      this.visibleApps = new ObservableCollection<Global.App>(this.availableApps);

      //Force update the UI
      Bindings.Update();
    }

    /// <summary>
    /// Run a fuzzy-search against the apps
    /// </summary>
    /// <param name="query">The user-provided query</param>
    /// <returns>Matching apps</returns>
    private List<Global.App> search(string query)
    {
      //Compute match ratio
      Dictionary<Global.App, int> unorderedResults = new Dictionary<Global.App, int>();
      foreach (Global.App app in this.availableApps)
      {
        //Compute the match ratio
        int ratio = Fuzz.Ratio(query, app.DisplayName);

        //Add to results if the ratio is at least threshold
        if (ratio >= threshold)
        {
          unorderedResults.Add(app, ratio);
        }
      }

      //Sort by ratio and extract keys
      List<Global.App> orderedResults = unorderedResults.OrderBy(result => result.Value).ToDictionary(result => result.Key, result => result.Value).Keys.ToList();

      return orderedResults;
    }

    /// <summary>
    /// Update the app view in a way that avoids UI flickering
    /// </summary>
    /// <param name="updated">The updated list of apps</param>
    private void updateApps(List<Global.App> updated)
    {
      //Add new items
      updated.ForEach(result =>
      {
        //If the result isn't already in the apps, add it
        if (!visibleApps.Contains(result))
        {
          visibleApps.Add(result);
        }
      });

      //Remove old items
      visibleApps.ToList().ForEach(app =>
      {
        //If the app isn't in the results, remove it
        if (!updated.Contains(app))
        {
          visibleApps.Remove(app);
        }
      });
    }

    /// <summary>
    /// Launch an app
    /// </summary>
    /// <param name="app">The app to launch</param>
    private async void launch(Global.App app)
    {
      //Get the credential for the platform
      Credential credential = await StateManager.state.client.GetCredentialAsync(app.PlatformID, StateManager.state.generalSettings.accountID);

      //Launch
      LaunchManager.Launch(app.ID, credential.Username, credential.Password, credential.MfaSecret);
    }

    private void GridView_ItemClick(object sender, ItemClickEventArgs e)
    {
      //Cast to app
      Global.App app = e.ClickedItem as Global.App;

      //Launch
      this.launch(app);
    }

    private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      //If search is empty, reset results
      if (searchBox.Text.Length == 0)
      {
        updateApps(this.availableApps);
      }
      //Otherwise perform a fuzzy search
      else
      {
        //Search for apps
        updateApps(search(searchBox.Text));
      }
    }
  }
}
