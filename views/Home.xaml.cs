using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace orange_thallium.views
{
  public sealed partial class Home : Page
  {
    public Home()
    {
      this.InitializeComponent();
    }

    /// <summary>
    /// Launch the apps page
    /// </summary>
    private void LaunchApps()
    {
      //Get the apps menu item
      MenuItem apps = MainPage.items.Where(item => item.Key == "apps").FirstOrDefault();

      //Attempt to navigate to the apps page
      MainPage.instance.AttemptNavigation(apps);
    }

    private void login_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
      //Launch the apps page
      this.LaunchApps();
    }

    private void startApp_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
    {
      //Launch the apps page
      this.LaunchApps();
    }
  }
}
