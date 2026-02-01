using System.Text.Json;
using System.Text.Json.Serialization;

namespace SW_PortalProprietario.Application.Auxiliar
{
    public class EncryptConverter : JsonConverter<string>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();

        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(Encrypt($"{value}"));
        }

        private string Encrypt(string plainText)
        {
            return EncryptDecryptProvider.Encrypt(plainText);
        }

        private string Decrypt(string cipherText)
        {
            return EncryptDecryptProvider.Decrypt(cipherText);
        }

    }
}
