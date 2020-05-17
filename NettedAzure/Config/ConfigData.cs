using System.Xml.Serialization;

namespace NettedAzure.Config
{
    public class ConfigData
    {
        [XmlElement]
        public string PrincipalId;
        [XmlElement]
        public string PrincipalKey;
        [XmlElement]
        public string TenantId;
        [XmlElement]
        public string SubscriptionId;
    }
}
