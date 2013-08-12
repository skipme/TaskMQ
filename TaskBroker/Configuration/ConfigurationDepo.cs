using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Configuration
{
    public class ConfigurationDepo
    {
        const string key_main = "JC_MAIN";
        const string key_modules = "JC_MODULES";
        const string key_assemblys = "JC_ASSEMBLYS";

        public FileContentArchive.ContentVersionStorage versions;
        public Dictionary<string, string> jsonConfigurations = new Dictionary<string, string>();

        public ConfigurationDepo()
        {
            versions = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage("conf-json.zip"));
        }

        public Configuration.ConfigurationBroker GetNewestConfigurationVersion()
        {
            Configuration.ConfigurationBroker bc;
            string errors;

            string json = versions.GetLatestVersion(key_main);
            if (json == null)
                return null;
            
            if(ConfigurationValidation.ValidateMain(ref json, out errors, out bc))
            {
                return bc;
            }
            return null;
        }


        public void RegisterConfiguration(string id, string body)
        {
            if (jsonConfigurations.ContainsKey(id))
                jsonConfigurations[id] = body;
            else
                jsonConfigurations.Add(id, body);
        }
        public bool ValidateAndCommitMain(string id, out string errors)
        {
            if (id == null || !jsonConfigurations.ContainsKey(id))
            {
                errors = "id not present in commit queue";
                return false;
            }
            else
            {
                string json = jsonConfigurations[id];
                bool vok = ConfigurationValidation.ValidateMain(ref json, out errors);

                versions.AddVersion(key_main, json);
            }

            errors = "ok";
            return true;
        }


        public bool ValidateAndCommitMods(string id, out string errors)
        {
            if (id == null || !jsonConfigurations.ContainsKey(id))
            {
                errors = "id not present in commit queue";
                return false;
            }
            else
            {
                string json = jsonConfigurations[id];
                bool vok = ConfigurationValidation.ValidateMain(ref json, out errors);

                versions.AddVersion(key_modules, json);
            }

            errors = "ok";
            return true;
        }
    }
}
