using System.Configuration;

namespace DiRTTelemetryErrorFix.Config
{
    public class MonitoredProcessesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("MonitoredProcesses", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(MonitoredProcessCollection), AddItemName = "add")]
        public MonitoredProcessCollection MonitoredProcesses
        {
            get
            {
                return (MonitoredProcessCollection)base["MonitoredProcesses"];
            }
        }
    }

    public class MonitoredProcessCollection : ConfigurationElementCollection
    {
        public MonitoredProcessElement this[int index]
        {
            get { return (MonitoredProcessElement)BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new MonitoredProcessElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MonitoredProcessElement)element).Name;
        }
    }

    public class MonitoredProcessElement : ConfigurationElement
    {
        public MonitoredProcessElement() { }

        public MonitoredProcessElement(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", DefaultValue = "", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }
    }


}
