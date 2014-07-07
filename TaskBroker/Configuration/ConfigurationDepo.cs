using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaskUniversum;

namespace TaskBroker.Configuration
{
    public class ConfigurationDepot
    {
        ILogger logger = TaskUniversum.ModApi.ScopeLogger.GetClassLogger();

        const string key_main = "JC_MAIN";
        const string key_modules = "JC_MODULES";
        const string key_assemblys = "JC_ASSEMBLYS";
        const string conf_archive = "conf-json.zip";

        public FileContentArchive.ContentVersionStorage versions;
        public Dictionary<string, string> jsonConfigurations = new Dictionary<string, string>();

        public ConfigurationDepot()
        {
            versions = new FileContentArchive.ContentVersionStorage(new FileContentArchive.ZipStorage(conf_archive));
        }
        public void ClearConfigurationsForCommit()
        {
            jsonConfigurations.Clear();
        }

        public Configuration.ConfigurationBroker GetNewestMainConfiguration()
        {
            Configuration.ConfigurationBroker bc;
            string errors;

            string json = versions.GetLatestVersion(key_main);
            if (json == null)
                return null;

            if (ConfigurationValidation.ValidateMain(ref json, out errors, out bc))
            {
                return bc;
            }
            logger.Error(errors);
            return null;
        }
        public Configuration.ConfigurationAssemblys GetNewestAssemblysConfiguration()
        {
            Configuration.ConfigurationAssemblys bc;
            string errors;

            string json = versions.GetLatestVersion(key_assemblys);
            if (json == null)
                return null;

            if (ConfigurationValidation.ValidateAssemblys(ref json, out errors, out bc))
            {
                return bc;
            }
            logger.Error(errors);
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

                try
                {
                    versions.AddVersion(key_main, json);
                    jsonConfigurations.Remove(id);
                }
                catch (Exception e)
                {
                    // archive errors // opened in another app
                    errors = e.Message;
                    return false;
                }
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
                bool vok = ConfigurationValidation.ValidateMods(ref json, out errors);
                try
                {
                    versions.AddVersion(key_modules, json);
                }
                catch (Exception e)
                {
                    // archive errors // opened in another app
                    errors = e.Message;
                    return false;
                }
            }

            errors = "ok";
            return true;
        }

        public bool ValidateAndCommitAssemblies(string id, out string errors)
        {
            if (id == null || !jsonConfigurations.ContainsKey(id))
            {
                errors = "id not present in commit queue";
                return false;
            }
            else
            {
                string json = jsonConfigurations[id];
                bool vok = ConfigurationValidation.ValidateAssemblys(ref json, out errors);
                try
                {
                    versions.AddVersion(key_assemblys, json);
                }
                catch (Exception e)
                {
                    // archive errors // opened in another app
                    errors = e.Message;
                    return false;
                }
            }

            errors = "ok";
            return true;
        }
    }
}
