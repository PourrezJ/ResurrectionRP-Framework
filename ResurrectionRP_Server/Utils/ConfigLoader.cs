using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ResurrectionRP_Server
{
    public class Config
    {
        public static Config LoadConfig()
        {
            var config = new Config();
            var baseDir = "resources" + Path.DirectorySeparatorChar + "resurrectionrp" + Path.DirectorySeparatorChar + "Server" + Path.DirectorySeparatorChar + "appsettings.json";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(baseDir, optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            foreach(var dat in configuration.AsEnumerable())
            {
                _settings.Add(dat.Key, dat.Value);
            }

            return config;
        }

        private static Dictionary<string, object> _settings;
        internal static Dictionary<string, object> Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Dictionary<string, object>();
                    LoadConfig();
                }
                return _settings;
            }
            set
            {
                if (_settings == null) _settings = new Dictionary<string, object>();
                _settings = value;
            }
        }

        public static T GetSetting<T>(string settingName)
        {
            if (Settings != null &&
                Settings.ContainsKey(settingName))
            {
                var val = Settings[settingName];

                T output;

                if (val != null)
                {
                    try
                    {
                        output = (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                        output = (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        output = (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
                    }

                    Settings[settingName] = val;
                }
                else
                {
                    output = (T)val;
                }

                return output;
            }

            return default(T);
        }
    }
}