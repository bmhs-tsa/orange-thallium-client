using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Security.Credentials;

namespace orange_thallium.classes
{
  class PasswordCredentialConverter : JsonConverter<PasswordCredential>
  {
    public override PasswordCredential Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      //Get the raw secret
      string rawSecret = reader.GetString();

      //Instantiate a new password credential
      PasswordCredential credential = new PasswordCredential(SettingsManager.OidcClientSecretKey, SettingsManager.OidcClientSecretUsername, rawSecret);

      return credential;
    }

    public override void Write(Utf8JsonWriter writer, PasswordCredential value, JsonSerializerOptions options)
    {
      //Write a null value
      writer.WriteNullValue();
    }
  }
}
