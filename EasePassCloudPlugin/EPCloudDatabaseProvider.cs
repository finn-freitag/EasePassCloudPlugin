using EasePassExtensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace EasePassCloudPlugin
{
    public class EPCloudDatabaseProvider : IDatabaseProvider
    {
        public string SourceName => "Ease Pass Cloud";

        public Uri SourceIcon => Icon.GetIconUri();

        public bool ExternalConfigEditingSupport => EPCloudConfigLoader.LoadConfigurations() != null;

        public IDatabaseSource[] GetDatabases()
        {
            var config = EPCloudConfigLoader.LoadConfigurations();
            if (config == null)
                return [];

            return config.AccessTokens
                .Select(token => new EPCloudDatabase(config.Host, token, config.SaveReadonlyOfflineCopies))
                .ToArray();
        }

        public string GetConfigurationJSON()
        {
            var config = EPCloudConfigLoader.LoadConfigurations();
            if (config == null)
                return string.Empty;
            return System.Text.Json.JsonSerializer.Serialize(config, EPCloudConfigLoader.options);
        }

        public bool SetConfigurationJSON(string configJson)
        {
            if(string.IsNullOrEmpty(configJson))
                return false;
            try
            {
                var config = System.Text.Json.JsonSerializer.Deserialize<EPCloudConfig>(configJson);
                if (config == null)
                    return false;
                EPCloudConfigLoader.SaveConfigurations(config);
                Logger.Log("Saved config json");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetSampleJsonConfig()
        {
            return "Welcome to Ease Pass Cloud!\nPlease visit your Ease Pass Cloud\nWebinterface to configure\nyour database connections.\n\nLogin and navigate to the\nsection \"Ease Pass Config\".\nCopy the contents in here.";
        }

        public void OpenExternalConfigEditor()
        {
            var config = EPCloudConfigLoader.LoadConfigurations();
            if (config == null)
                return;
            Process.Start(new ProcessStartInfo()
            {
                FileName = (config.Host.StartsWith("http") ? "" : "http://") + config.Host,
                UseShellExecute = true
            });
        }
    }
}
