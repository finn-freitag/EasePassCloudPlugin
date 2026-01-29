using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasePassCloudPlugin
{
    internal class EPCloudConfigLoader
    {
        public static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true,
            // This converts the Enum to "SFTP" instead of 1
            Converters = { new JsonStringEnumConverter() },
            // Allows accessing internal classes
            IncludeFields = true
        };

        public static EPCloudConfig[] LoadConfigurations()
        {
            string data = ConfigurationStorage.Instance.LoadString("config");
            if (string.IsNullOrEmpty(data))
                return [];

            string json = Obfuscator.Decrypt(data);
            if (json.TrimStart().StartsWith("["))
                return JsonSerializer.Deserialize<EPCloudConfig[]>(json, options) ?? [];
            else
            {
                var singleObj = JsonSerializer.Deserialize<EPCloudConfig>(json, options);
                if (singleObj != null)
                    return [singleObj];
                else
                    return [];
            }
        }

        public static void SaveConfigurations(EPCloudConfig[] config)
        {
            string json = JsonSerializer.Serialize(config, options);
            string data = Obfuscator.Encrypt(json);
            ConfigurationStorage.Instance.SaveString("config", data);
        }
    }
}
