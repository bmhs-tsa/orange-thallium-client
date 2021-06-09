using orange_thallium.classes;
using orange_thallium.views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace orange_thallium
{
  /// <summary>
  /// A MenuItem represents an item in the navigation menu
  /// </summary>
  public class MenuItem
  {
    /// <summary>
    /// A unique key used for identifying items
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The view to be displayed when a user invokes the item
    /// </summary>
    public Type View { get; set; }

    /// <summary>
    /// The icon displayed in the navigation menu
    /// </summary>
    public Symbol Icon { get; set; }

    /// <summary>
    /// The text displayed in the navigation menu
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Whether or not the user needs to authenticate before enabling the menu item
    /// </summary>
    public bool AuthRequired { get; set; } = false;

    /// <summary>
    /// Whether to omit displaying the menu item or not (Mainly for the settings item)
    /// </summary>
    public bool Omit { get; set; } = false;
  }
  
  public sealed partial class MainPage : Page
  {
    public static MainPage instance;
    public static Collection<MenuItem> items = new Collection<MenuItem>
    {
      new MenuItem
      {
        Key = "home",
        View = typeof(Home),
        Icon = Symbol.Home,
        Label = "Home"
      },
      new MenuItem
      {
        Key = "apps",
        View = typeof(Apps),
        Icon = (Symbol)0xE7FC,
        Label = "Apps",
        AuthRequired = true
      },
      new MenuItem
      {
        Key = "SettingsNavPaneItem",
        View = typeof(Settings),
        Omit = true
      }
    };
    public bool isLoading = false;
    private MenuItem previousItem;

    /// <summary>
    /// The main frame visibility property
    /// </summary>
    private string mainFrameVisisblity
    {
      get
      {
        return this.isLoading ? "Collapsed" : "Visible";
      }
    }

    /// <summary>
    /// The progress ring visibility property 
    /// </summary>
    private string progressRingVisibility
    {
      get
      {
        return this.isLoading ? "Visible" : "Collapsed";
      }
    }

    public MainPage()
    {
      this.InitializeComponent();

      //Update instance
      instance = this;

      //Enable navigation caching
      this.NavigationCacheMode = NavigationCacheMode.Enabled;

      //Update menu items
      this.UpdateItems();

      //Load settings
      SettingsManager.LoadSettings();

      //Get the first menu item
      MenuItem item = items.FirstOrDefault();

      //Set the selected item
      this.SetSelectedItem(item);

      //Navigate
      this.AttemptNavigation(item);
    }

    /// <summary>
    /// Update menu items
    /// </summary>
    private void UpdateItems()
    {
      //Convert MenuItems to NavigationViewItems
      List<NavigationViewItem> navigationViewItems = new List<NavigationViewItem>();

      foreach (MenuItem item in items)
      {
        if (item.Omit)
        {
          continue;
        }

        //Create a new NavigationViewItem
        NavigationViewItem navigationViewItem = new NavigationViewItem
        {
          Name = item.Key,
          Icon = new SymbolIcon(item.Icon),
          Content = new TextBlock
          {
            Text = item.Label
          }
        };

        //Append to the array
        navigationViewItems.Add(navigationViewItem);
      }

      navigationView.MenuItemsSource = navigationViewItems;
    }

    /// <summary>
    /// Set the navigation view selected item
    /// </summary>
    /// <param name="item"></param>
    private void SetSelectedItem(MenuItem item)
    {
      //Set the selected item
      List<NavigationViewItem> sourceItems = navigationView.MenuItemsSource as List<NavigationViewItem>;
      navigationView.SelectedItem = sourceItems.Find(sourceItem => sourceItem.Name == item.Key);
    }

    /// <summary>
    /// Set the app loading state
    /// </summary>
    /// <param name="loading">Whether or not the app is loading</param>
    private void SetLoading(bool loading)
    {
      //Update loading
      this.isLoading = loading;

      //Force update the UI
      Bindings.Update();
    }

    /// <summary>
    /// Attempt to authenticate the user
    /// </summary>
    /// <returns>Whether the user successfully authenticated</returns>
    private async Task<bool> AttemptAuthentication()
    {
      //Enable loading
      this.SetLoading(true);

      //Attempt authentication - if successful, return
      if (await OidcManager.Authenticate())
      {
        //Disable loading
        this.SetLoading(false);

        return true;
      }
      //If unsuccessful, ask the user if they would like to try again or go back
      else
      {
        //Prompt the user
        ContentDialog dialog = new ContentDialog
        {
          Title = "Authentication Failed",
          Content = "You must log in to view this page; would you like to do so?",
          PrimaryButtonText = "Try Again",
          SecondaryButtonText = "Cancel"
        };

        ContentDialogResult result = await dialog.ShowAsync();

        //Try again
        if (result == ContentDialogResult.Primary)
        {
          //Recur
          return await this.AttemptAuthentication();
        }
        //Cancel
        else
        {
          //Disable loading
          this.SetLoading(false);

          return false;
        }
      }
    }

    /// <summary>
    /// Attempt to navigate
    /// </summary>
    /// <param name="item">The menu item to attempt navigation for</param>
    public async void AttemptNavigation(MenuItem item)
    {
      //If not authenticated but the view needs authentication, attempt authentication
      if (!StateManager.state.authenticated && item.AuthRequired)
      {
        //Attempt authentication - if not successful, cancel navigation
        if (!await this.AttemptAuthentication())
        {
          //Navigate to previous view
          this.mainFrame.Navigate(this.previousItem.View);

          //Set the selected item to the previous item
          this.SetSelectedItem(this.previousItem);

          return;
        }
      }

      //Navigate
      mainFrame.Navigate(item.View);

      //Save menu item for later
      this.previousItem = item;
    }

    private void navigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
      //Get the invoked item
      NavigationViewItem invokedItem = args.InvokedItemContainer as NavigationViewItem;

      //Get the corresponding menu item
      MenuItem item = items.Where(item => item.Key == invokedItem.Name).FirstOrDefault();

      //Attempt navigation
      this.AttemptNavigation(item);
    }

    private void navigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
      if (mainFrame.CanGoBack)
      {
        mainFrame.GoBack();
      }
    }
  }
}
