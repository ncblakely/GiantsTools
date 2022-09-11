namespace Giants.Launcher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;
    using Newtonsoft.Json;

    public class Config
    {
        private const string defaultConfigFileName = "GiantsDefault.config";
        private const string playerConfigFileName = "Giants.config";
        private bool dirty = false;

        private IDictionary<string, dynamic> defaultConfig = new Dictionary<string, dynamic>();
        private IDictionary<string, dynamic> userConfig = new Dictionary<string, dynamic>();

        public void Read()
        {
            string defaultConfigFilePath = this.GetDefaultConfigPath();
            if (!File.Exists(defaultConfigFilePath))
            {
                string message = string.Format(Resources.ErrorNoConfigFile, defaultConfigFilePath);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
                return;
            }

            this.defaultConfig = ReadConfig(defaultConfigFilePath);

            string userConfigFilePath = this.GetUserConfigPath();
            if (File.Exists(userConfigFilePath))
            {
                this.userConfig = ReadConfig(userConfigFilePath);
            }
        }

        public void Write()
        {
            if (!this.dirty)
            {
                return;
            }

            try
            {
                string userConfigFilePath = this.GetUserConfigPath();

                Directory.CreateDirectory(Path.GetDirectoryName(userConfigFilePath));

                using (var streamWriter = new StreamWriter(userConfigFilePath, append: false))
                {
                    string serializedConfig = JsonConvert.SerializeObject(this.userConfig, Formatting.Indented);
                    streamWriter.Write(serializedConfig);
                }

                this.dirty = false;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unhandled exception saving updated configuration: {e}");
            }
        }

        public bool TryGetObject(string section, string key, object defaultValue, out object value)
        {
            value = defaultValue;

            if (this.userConfig.ContainsKey(section))
            {
                dynamic sectionObject = this.userConfig[section];
                if (sectionObject != null && sectionObject.ContainsKey(key))
                {
                    value = sectionObject[key];
                    return true;
                }
            }

            if (this.defaultConfig.ContainsKey(section))
            {
                dynamic sectionObject = this.defaultConfig[section];
                if (sectionObject != null && sectionObject.ContainsKey(key))
                {
                    value = sectionObject[key];
                    return true;
                }
            }

            return false;
        }

        public bool TryGetString(string section, string key, string defaultValue, out string value)
        {
            value = defaultValue;

            if (this.TryGetObject(section, key, defaultValue, out object objValue))
            {
                value = objValue.ToString();
                return true;
            }

            return false;
        }

        public bool TryGetBool(string section, string key, bool defaultValue, out bool value)
        {
            value = defaultValue;

            if (this.TryGetObject(section, key, defaultValue, out object objValue))
            {
                return bool.TryParse(objValue.ToString(), out value);
            }

            return false;
        }

        public void SetValue(string section, string key, dynamic value)
        {
            if (!this.userConfig.ContainsKey(section))
            {
                this.userConfig.Add(section, new Dictionary<string, dynamic>());
            }

            dynamic sectionObject = this.userConfig[section];
            if (sectionObject != null && sectionObject.ContainsKey(key))
            {
                sectionObject[key] = value;
            }
            else
            {
                sectionObject.Add(key, value);
            }

            this.dirty = true;
        }

        private static IDictionary<string, dynamic> ReadConfig(string filePath)
        {
            string fileContents = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(fileContents);
        }

        private string GetDefaultConfigPath()
        {
            return defaultConfigFileName;
        }

        private string GetUserConfigPath()
        {
            string applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            return Path.Combine(applicationDataPath, "Giants", playerConfigFileName);
        }
    }
}
