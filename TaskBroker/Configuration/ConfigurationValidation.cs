using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Configuration
{
    public class ConfigurationValidation
    {
        public static bool ValidateMain(string json, out string errors)
        {
            ConfigurationBroker bc = null;
            try
            {
                bc = ConfigurationBroker.DeSerialiseJson(json);
            }
            catch (Exception e)
            {
                errors = "Configuration broken: " + e.Message;
                return false;
            }
            errors = "ok";
            return true;
        }
    }
}
