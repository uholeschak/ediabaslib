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

        public override EdiabasNet Ediabas
        {
            get
            {
                return base.Ediabas;
            }
            set
            {
                base.Ediabas = value;

                string prop = EdiabasProtected?.GetConfigProperty("RemoteHost_" + RplusSectionProtected);
                if (!string.IsNullOrEmpty(prop))
                {
                    RemoteHostProtected = prop;
                }

                prop = EdiabasProtected?.GetConfigProperty("Port_" + RplusSectionProtected);
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusPort = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("TimeoutFunction_" + RplusSectionProtected);
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusFunctionTimeout = (int)EdiabasNet.StringToValue(prop);
                }

                prop = EdiabasProtected?.GetConfigProperty("IcomEnetRedirect_" + RplusSectionProtected);
                if (!string.IsNullOrEmpty(prop))
                {
                    RplusIcomEnetRedirect = EdiabasNet.StringToValue(prop) != 0;
                }
            }
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

                RplusSectionProtected = DefaultRplusSection;
                if (!string.IsNullOrEmpty(IfhNameProtected))
                {
                    string[] parts = IfhNameProtected.Split(':');

                    if (parts.Length > 1)
                    {
                        RplusSectionProtected = parts[1].Trim();
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

                RplusIcomEnetRedirect = string.Compare(RplusSectionProtected, DefaultRplusSection, StringComparison.OrdinalIgnoreCase) == 0;
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
