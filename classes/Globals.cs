using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Global
{
  public class App
  {
    public string ID { get; set; }
    public string PlatformID { get; set; }
    public string Script { get; set; }
    public string DisplayName { get; set; }
    public Uri Logo { get; set; }
  }

  public class Globals
  {
    //The app key
    public static string appKey = "app";

    //The credential keys
    public static string usernameKey = "username";
    public static string passwordKey = "password";
    public static string mfaCodeKey = "mfaCode";

    //Dictionary of supported apps
    public static List<App> apps = new List<App>()
    {
      new App
      {
        ID = "epic-games",
        PlatformID = "epic-games",
        Script = "scripts/epic-games/login.ahk",
        DisplayName = "Epic Games",
        Logo = new Uri("ms-appx:///logos/epic-games.png")
      },
      new App
      {
        ID = "steam",
        PlatformID = "steam",
        Script = "scripts/steam/login.ahk",
        DisplayName = "Steam",
        Logo = new Uri("ms-appx:///logos/steam.png")
      },
      new App
      {
        ID = "valorant",
        PlatformID = "riot",
        Script = "scripts/valorant/login.ahk",
        DisplayName = "Valorant",
        Logo = new Uri("ms-appx:///logos/valorant.png")
      },
    };

    //Get shared entropy
    private static byte[] getEntropy()
    {
      //Get resource
      Collection<string> resources = new Collection<string>(Assembly.GetExecutingAssembly().GetManifestResourceNames());

      //Get the entropy resources
      List<string> entropyResources = resources.Where(resource => resource.EndsWith("entropy.key")).ToList();

      //Prevent mis-configurations
      if (entropyResources.Count != 1)
      {
        throw new Exception("Invalid entropy resource configuration!");
      }

      //Get a stream for the embedded entropy
      Stream entropyStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(entropyResources.FirstOrDefault());

      //Handle invalid streams
      if (entropyStream == null)
      {
        throw new Exception("Entropy is invalid");
      }

      //Read the entropy
      byte[] entropy = new byte[entropyStream.Length];
      entropyStream.Read(entropy, 0, (int)entropyStream.Length);

      return entropy;
    }

    //Encrypt data
    public static string Encrypt(string plaintext)
    {
      //Get bytes
      byte[] bytes = Encoding.UTF8.GetBytes(plaintext);

      //Get the entropy
      byte[] entropy = getEntropy();

      //Encrypt
      byte[] encrypted = ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser);

      //Get base64 string
      return Convert.ToBase64String(encrypted);
    }

    //Decrypt data
    public static string Decrypt(string ciphertext)
    {
      //Get bytes
      byte[] bytes = Convert.FromBase64String(ciphertext);

      //Get the entropy
      byte[] entropy = getEntropy();

      //Decrypt
      byte[] decrypted = ProtectedData.Unprotect(bytes, entropy, DataProtectionScope.CurrentUser);

      //Get string
      return Encoding.UTF8.GetString(decrypted);
    }
  }
}
