using System;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EdInterfaceAds : EdInterfaceObd
    {
        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop = EdiabasProtected.GetConfigProperty("AdsComPort");
                if (prop != null)
                {
                    ComPortProtected = prop;
                }
            }
        }

        public override string InterfaceType
        {
            get
            {
                return "ADS";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 209;
            }
        }

        public override string InterfaceName
        {
            get
            {
                return "ADS";
            }
        }

        public override bool IsValidInterfaceName(string name)
        {
            return IsValidInterfaceNameStatic(name);
        }

        public new static bool IsValidInterfaceNameStatic(string name)
        {
            string[] nameParts = name.Split(':');
            if (nameParts.Length > 0 && string.Compare(nameParts[0], "ADS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        protected override bool AdapterEcho
        {
            get
            {
                return false;
            }
        }
    }
}
