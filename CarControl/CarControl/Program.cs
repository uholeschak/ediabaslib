using System;

using System.Collections.Generic;
using System.Windows.Forms;

namespace CarControl
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    uint launchCode = uint.Parse(args[0]);
                    WinAPI.SignalStarted(launchCode);
                }
                catch (Exception)
                {
                }
            }
            Application.Run(new MainForm());
        }
    }
}
