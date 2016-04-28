using System;
using System.Windows.Forms;

namespace DiRTTelemetryErrorFix
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool silent = false;
            foreach(String arg in Environment.GetCommandLineArgs())
            {
                if (arg.Equals("-silent"))
                {
                    silent = true;
                }
            }
            Application.Run(new TrayIconApplicationContext(silent));
        }
    }
}
