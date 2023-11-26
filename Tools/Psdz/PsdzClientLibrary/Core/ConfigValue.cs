namespace PsdzClientLibrary.Core
{
    internal class ConfigValue
    {
        private object defaultValue;

        public object Value { get; set; }

        public string Origin { get; set; }

        public object DefaultValue
        {
            get
            {
                return defaultValue;
            }
            set
            {
                if (!object.Equals(defaultValue, value))
                {
                    defaultValue = value;
                    IsLogged = false;
                }
            }
        }

        public bool IsLogged { get; set; }

        public ConfigValue(object value, string origin)
        {
            Value = value;
            Origin = origin;
        }
    }
}
