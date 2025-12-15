using Microsoft.Win32;
using System;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace EdiabasLibConfigTool
{
    [SupportedOSPlatform("windows")]
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            int netFrameworkVersion = GetNetFrameworkVersion();
            if (netFrameworkVersion < 0x481)
            {
                string message = string.Format(Resources.Strings.NetFrameworkMissing, "4.8.1");
                MessageBox.Show(message, Resources.Strings.TitleError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Application.Run(new FormMain());
        }

        public static int GetNetFrameworkVersion()
        {
            try
            {
                using (RegistryKey localMachine32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey ndpKey = localMachine32.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                    {
                        if (ndpKey != null)
                        {
                            object releaseValue = ndpKey.GetValue("Release");
                            if (releaseValue is int releaseKey)
                            {
                                return ConvertNetFrameworkVersion(releaseKey);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return -1;
            }
            return -1;
        }

        public static int ConvertNetFrameworkVersion(int releaseKey)
        {
            if (releaseKey >= 533320)
            {
                return 0x481;
            }

            if (releaseKey >= 528040)
            {
                return 0x480;
            }

            if (releaseKey >= 461808)
            {
                return 0x472;
            }

            if (releaseKey >= 461308)
            {
                return 0x471;
            }

            if (releaseKey >= 460798)
            {
                return 0x470;
            }

            if (releaseKey >= 394802)
            {
                return 0x462;
            }

            if (releaseKey >= 394254)
            {
                return 0x461;
            }

            if (releaseKey >= 393295)
            {
                return 0x460;
            }

            if (releaseKey >= 379893)
            {
                return 0x452;
            }

            if (releaseKey >= 378675)
            {
                return 0x451;
            }

            if (releaseKey >= 378389)
            {
                return 0x450;
            }

            return 0x000;
        }
    }
}
