/**
 * Based on https://github.com/IdentityModel/IdentityModel.OidcClient.Samples/blob/main/WpfWebView2/WpfWebView2/WpfEmbeddedBrowser.cs
 */

using IdentityModel.OidcClient.Browser;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace orange_thallium
{
  class Browser : IBrowser
  {
    private static AppWindow appWindow;

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
      //Create a semaphore
      SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0, 1);

      //Close existing app windows
      if (appWindow != null)
      {
        await appWindow.CloseAsync();
      }

      //Create a new app window
      appWindow = await AppWindow.TryCreateAsync();

      //Customize the app window
      appWindow.RequestMoveAdjacentToCurrentView();
      appWindow.RequestSize(new Size(1000, 800));
      appWindow.Title = "Login";

      //Close the window if the main window is closed before the user signs in
      ApplicationView.GetForCurrentView().Consolidated += async (ApplicationView sender, ApplicationViewConsolidatedEventArgs args) =>
      {
        await appWindow.CloseAsync();
      };

      //Create a new webview
      WebView webView = new WebView(WebViewExecutionMode.SameThread);

      //Release the semaphore and set the result on the desired navigation
      BrowserResult result = new BrowserResult
      {
        ResultType = BrowserResultType.UserCancel
      };

      webView.NavigationStarting += async (WebView sender, WebViewNavigationStartingEventArgs args) =>
      {
        //Filter out other navigations
        if (args.Uri.AbsoluteUri.StartsWith(options.EndUrl))
        {
          //Cancel to prevent the page loading
          args.Cancel = true;

          //Set the response type
          result = new BrowserResult
          {
            ResultType = BrowserResultType.Success,
            Response = args.Uri.AbsoluteUri
          };

          //Release the semaphore and close the window
          semaphoreSlim.Release();
          await appWindow.CloseAsync();
        }
      };

      //Release the semaphore on close
      appWindow.Closed += (AppWindow sender, AppWindowClosedEventArgs args) =>
      {
        semaphoreSlim.Release();
      };

      //Add the frame to the window
      ElementCompositionPreview.SetAppWindowContent(appWindow, webView);

      //Show the app window
      await appWindow.TryShowAsync();

      //Navigate to the start URL
      webView.Navigate(new Uri(options.StartUrl));

      //Wait for the semaphore
      await semaphoreSlim.WaitAsync();

      //Clear cookies
      await WebView.ClearTemporaryWebDataAsync();

      //Switch back to the main view
      int coreWindowID = ApplicationView.GetApplicationViewIdForWindow(CoreApplication.MainView.CoreWindow);
      await ApplicationViewSwitcher.TryShowAsStandaloneAsync(coreWindowID);

      //Return the result
      return result;
    }
  }
}