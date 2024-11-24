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
            Application.Run(new FormMain());
        }
    }
}
