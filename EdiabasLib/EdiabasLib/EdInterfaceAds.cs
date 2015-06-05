using System;

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

        public static new bool IsValidInterfaceNameStatic(string name)
        {
            if (string.Compare(name, "ADS", StringComparison.OrdinalIgnoreCase) == 0)
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
