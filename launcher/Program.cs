using Global;
using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace launcher
{
  class Program
  {
    /// <summary>
    /// The log writer (Use instead of Debug)
    /// </summary>
    static StreamWriter writer;

    static void Main(string[] args)
    {
      //Get the log path
      string logPath;

#if DEBUG
      logPath = "./log.txt";
#else
      logPath = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, "log.txt");
#endif

      //Initialize the log file writer
      writer = new StreamWriter(logPath, true);

      //Log
      writer.WriteLine("{0}{0}Launching at {1}", writer.NewLine, DateTime.Now.ToString());

      //Log all exceptions
      AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;

      //Get the script
      string script;

#if DEBUG
      script = "scripts/debug.ahk";
#else
      //Get the app ID
      string appID = ApplicationData.Current.LocalSettings.Values[Globals.appKey] as string;

      //Ensure the app is supported
      if (!Globals.apps.Exists(app => app.ID == appID))
      {
        throw new Exception(string.Format("Invalid app '{0}'", appID));
      }

      //Get the app script path
      script = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Globals.apps.Find(app => app.ID == appID).Script);
#endif

      //Log
      writer.WriteLine("Using app script '{0}'", script);

      //Get the credentials
      string username;
      string password;
      string mfaCode = null;

#if DEBUG
      username = "[USERNAME]";
      password = "[PASSWORD]";
      mfaCode = "123456";
#else
      //Read the credentials
      username = ReadCredentialField(Globals.usernameKey);
      password = ReadCredentialField(Globals.passwordKey);

      //If the MFA code was provided, read it too
      if (ApplicationData.Current.LocalSettings.Values.ContainsKey(Globals.mfaCodeKey))
      {
        mfaCode = ReadCredentialField(Globals.mfaCodeKey);
      }
#endif

      //Log
      writer.WriteLine("Decrypted credentials");

      //Instantiate the process info
      ProcessStartInfo info = new ProcessStartInfo
      {
        Arguments = script,
        FileName = @"C:\Program Files\AutoHotkey\AutoHotkey.exe",
        UseShellExecute = false,
        WindowStyle = ProcessWindowStyle.Hidden
      };

      //Add environment variables
      info.EnvironmentVariables[Globals.usernameKey] = username;
      info.EnvironmentVariables[Globals.passwordKey] = password;
      if (mfaCode != null)
      {
        info.EnvironmentVariables[Globals.mfaCodeKey] = mfaCode;
      }

      //Start the script
      Process.Start(info);

      //Log
      writer.WriteLine("Launched app script, exiting");
      writer.Close();
    }

    /// <summary>
    /// Log all exceptions
    /// </summary>
    /// <param name="sender">Exception sender</param>
    /// <param name="e">Exception handler arguments</param>
    private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
    {
      writer.Write(e.Exception.ToString());
      writer.Close();
    }

    /// <summary>
    /// Read and decrypt a credential field passed by the UWP app
    /// </summary>
    /// <param name="key">The globally recognized credential field</param>
    /// <returns>The decrypted credential field</returns>
    private static string ReadCredentialField(string key)
    {
      //Read the encrypted field
      string encryptedField = ApplicationData.Current.LocalSettings.Values[key] as string;

      //Delete the encrypted field from the disk
      ApplicationData.Current.LocalSettings.Values[key] = "";

      //Decrypt the field
      string field = Globals.Decrypt(encryptedField);

      return field;
    }
  }
}
