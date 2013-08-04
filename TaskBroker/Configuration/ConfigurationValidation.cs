using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaskBroker.Configuration
{
    public class ConfigurationValidation
    {
        public static bool ValidateMain(ref string json, out string errors)
        {
            ConfigurationBroker cb;
            return ValidateMain(ref json, out errors, out cb);
        }
        public static bool ValidateMain(ref string json, out string errors, out ConfigurationBroker bc)
        {
            bc = null;
            try
            {
                bc = ConfigurationBroker.DeSerialiseJson(json);
                json = bc.SerialiseString();
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
