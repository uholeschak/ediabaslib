using System;
// ReSharper disable ConvertPropertyToExpressionBody

namespace EdiabasLib
{
    public class EdInterfaceRplus : EdInterfaceEnet
    {
        public const string DefaultRplusSection = "ICOM_P";

        public EdInterfaceRplus()
        {
            RplusSectionProtected = DefaultRplusSection;
        }

        public override string IfhName
        {
            get
            {
                return base.IfhName;
            }
            set
            {
                base.IfhName = value;

                if (!string.IsNullOrEmpty(IfhNameProtected))
                {
                    string[] parts = IfhNameProtected.Split(':');

                    if (parts.Length > 1)
                    {
                        RplusSectionProtected = parts[1].Trim();
                    }
                    else
                    {
                        RplusSectionProtected = DefaultRplusSection;
                    }

                    if (parts.Length > 2)
                    {
                        string configParams = parts[2];
                        string[] configParts = configParams.Split(';');
                        foreach (string configPart in configParts)
                        {
                            string[] subParts = configPart.Split('=');
                            if (subParts.Length == 2)
                            {
                                string key = subParts[0].Trim();
                                string val = subParts[1].Trim();
                                if (string.Compare(key, "RemoteHost", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    RemoteHostProtected = val;
                                    continue;
                                }

                                if (string.Compare(key, "Port", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    RplusPort = (int)EdiabasNet.StringToValue(val);
                                    continue;
                                }
                            }
                        }
                    }
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
                return "RPLUS:" + RplusSectionProtected;
            }
        }

        public override bool IsValidInterfaceName(string name)
        {
            return IsValidInterfaceNameStatic(name);
        }

        public new static bool IsValidInterfaceNameStatic(string name)
        {
            string[] nameParts = name.Split(':');
            if (nameParts.Length > 1 && string.Compare(nameParts[0], "RPLUS", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }
    }
}
