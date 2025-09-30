using System;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EdInterfaceRplus : EdInterfaceEnet
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
                string prop = EdiabasProtected.GetConfigProperty("Port");
                if (prop != null)
                {
                    RplusPort = (int)EdiabasNet.StringToValue(prop);
                }
            }
        }

        public override string InterfaceType
        {
            get
            {
                return "RPLUS";
            }
        }

        public override UInt32 InterfaceVersion
        {
            get
            {
                return 1795;
            }
        }

        public override string InterfaceName
        {
            get
            {
                return "RPLUS:ICOM_P";
            }
        }

        public override bool IsValidInterfaceName(string name)
        {
            return IsValidInterfaceNameStatic(name);
        }

        public new static bool IsValidInterfaceNameStatic(string name)
        {
            string[] nameParts = name.Split(':');
            if (nameParts.Length > 0 && string.Compare(nameParts[0], "RPLUS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }

        protected override bool RplusMode
        {
            get
            {
                return true;
            }
        }
    }
}
