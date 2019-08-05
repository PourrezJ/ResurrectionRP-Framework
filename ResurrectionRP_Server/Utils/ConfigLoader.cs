using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace ResurrectionRP.Server
{
    public class Config
    {
        private static Dictionary<string, CustomSetting> LoadSettings(List<MetaSetting> sets)
        {
            var dict = new Dictionary<string, CustomSetting>();

            if (sets == null) return dict;
            foreach (var setting in sets)
            {
                dict.Add(setting.Name, new CustomSetting()
                {
                    Value = setting.Value,
                    DefaultValue = setting.DefaultValue,
                    Description = setting.Description,
                });
            }

            return dict;
        }

        public static Config LoadConfig(string path)
        {
            var config = new Config();

            ResourceInfo resourceInfo;
            var xmlSer = new XmlSerializer(typeof(ResourceInfo));
            using (var str = File.OpenRead(path))
            {
                resourceInfo = (ResourceInfo)xmlSer.Deserialize(str);
            }

            if (resourceInfo.settings != null)
            {
                if (string.IsNullOrEmpty(resourceInfo.settings.Path))
                {
                    Settings = LoadSettings(resourceInfo.settings.Settings);
                }
                else
                {
                    var ser2 = new XmlSerializer(typeof(ResourceSettingsFile));

                    ResourceSettingsFile file;

                    using (var stream = File.Open(resourceInfo.settings.Path, FileMode.Open))
                        file = ser2.Deserialize(stream) as ResourceSettingsFile;

                    if (file != null)
                    {
                        Settings = LoadSettings(file.Settings);
                    }
                }
            }

            return config;
        }

        private static Dictionary<string, CustomSetting> _settings;
        internal static Dictionary<string, CustomSetting> Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new Dictionary<string, CustomSetting>();
                    var baseDir = "dotnet/resources" + Path.DirectorySeparatorChar + "netcoreapp2.2" + Path.DirectorySeparatorChar + "meta.xml";
                    LoadConfig(baseDir);
                }
                return _settings;
            }
            set
            {
                if (_settings == null) _settings = new Dictionary<string, CustomSetting>();
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

                if (!val.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(val.Value))
                        val.Value = val.DefaultValue;

                    try
                    {
                        output = (T)Convert.ChangeType(val.Value, typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException)
                    {
                        output = (T)Convert.ChangeType(val.DefaultValue, typeof(T), CultureInfo.InvariantCulture);
                    }
                    catch (FormatException)
                    {
                        output = (T)Convert.ChangeType(val.DefaultValue, typeof(T), CultureInfo.InvariantCulture);
                    }

                    val.CastObject = output;
                    val.HasValue = true;

                    Settings[settingName] = val;
                }
                else
                {
                    output = (T)val.CastObject;
                }

                return output;
            }

            return default(T);
        }

    }

    public struct CustomSetting
    {
        public string Value;
        public string DefaultValue;
        public string Description;

        public object CastObject;
        public bool HasValue;
    }

    [XmlRoot("meta"), Serializable]
    public class ResourceInfo
    {
        public ResourceSettingsMeta settings { get; set; }
    }

    [XmlRoot("settings")]
    public class ResourceSettingsMeta
    {
        [XmlAttribute("src")]
        public string Path { get; set; }

        // OR

        [XmlElement("setting")]
        public List<MetaSetting> Settings { get; set; }
    }


    [XmlRoot("settings")]
    public class ResourceSettingsFile
    {
        [XmlElement("setting")]
        public List<MetaSetting> Settings { get; set; }
    }

    public class MetaSetting
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        [XmlAttribute("default")]
        public string DefaultValue { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }
    }
}
