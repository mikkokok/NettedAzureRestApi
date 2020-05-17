using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace NettedAzure.Config
{
    class ConfigLoader
    {
        private const string Config = "config.xml";
        

        public ConfigData LoadConfig()
        {
            if (!File.Exists(Config))
                throw new Exception("Configuration file not found");

            using (var fileStream = new FileStream(Config, FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(ConfigData));
                return (ConfigData)xmlSerializer.Deserialize(fileStream);
            }
        }
    }
}
