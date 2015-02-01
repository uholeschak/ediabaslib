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

                string prop = ediabas.GetConfigProperty("AdsComPort");
                if (prop != null)
                {
                    comPort = prop;
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
            if (string.Compare(name, "ADS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        public EdInterfaceAds()
        {
        }

        protected override bool adapterEcho
        {
            get
            {
                return false;
            }
        }
    }
}
