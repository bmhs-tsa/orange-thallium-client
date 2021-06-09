using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace orange_thallium.classes
{
  class ApiMessageHandler : HttpClientHandler
  {
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      //Make the request
      HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

      //If not successful, display an error message
      if (!response.IsSuccessStatusCode)
      {
        //Get the body text
        string bodyText = await response.Content.ReadAsStringAsync();

        //Parse as JSON
        Error error = JsonConvert.DeserializeObject<Error>(bodyText);

        //Prompt the user
        ContentDialog dialog = new ContentDialog
        {
          Title = "API Error",
          Content = error.Message,
          CloseButtonText = "Close"
        };

        await dialog.ShowAsync();

        //Close
        Application.Current.Exit();
      }

      return response;
    }
  }
}
