using EasyHook;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSockHook;

namespace DiRTTelemetryErrorFix
{
    class TrayIconApplicationContext : ApplicationContext
    {
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private List<String> monitoredProcesses = new List<string>();
        private string currentProcessName;
        private int currentProcessId;

        public TrayIconApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            TrayIcon = new NotifyIcon();
            TrayIcon.Text = Properties.Resources.ApplicationName;
            TrayIcon.Icon = Properties.Resources.TrayIcon;
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // create simple context menu
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] { this.CloseMenuItem });
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            // add close menu item
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Text = Properties.Resources.CloseText;
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            TrayIcon.Visible = true;

            // start process monitor timer
            this.initMonitor();
        }

        private void initMonitor()
        {
            this.loadMonitoredProcesses();

            // check for existence of one of the processes, exit if none exist after 10 sec of waiting
            this.currentProcessId = -1;
            this.currentProcessName = null;
            for (int i = 0; i < 10; i++)
            {
                this.monitoredProcesses.ForEach(process =>
                {
                    foreach (Process proc in Process.GetProcessesByName(process))
                    {
                        this.currentProcessId = proc.Id;
                        this.currentProcessName = process;
                        break;
                    }
                });

                if (this.currentProcessId != -1)
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            if (this.currentProcessId == -1)
            {
                TrayIcon.ShowBalloonTip(0, Properties.Resources.ApplicationName, "No compatible processes running. App will close.", ToolTipIcon.Warning);
                Console.WriteLine(String.Format("No compatible processes running. App will close."));
                Application.Exit();
                return;
            }

            string tipText = String.Format(Properties.Resources.TrayIconBalloonTipFormatString, Properties.Resources.ApplicationName, this.currentProcessName);
            TrayIcon.ShowBalloonTip(0, Properties.Resources.ApplicationName, tipText, ToolTipIcon.Info);
            this.hookProcess(this.currentProcessId);
        }

        private void hookProcess(int processId)
        {
            string channelName = null;
            string hookAssyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "WinSockHook.dll");
            RemoteHooking.IpcCreateServer<HookCallbackHandler>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);
            RemoteHooking.Inject(processId, InjectionOptions.DoNotRequireStrongName, hookAssyPath, hookAssyPath, new Object[] { channelName, processId });

            // wait for host process termination
            try
            {
                do
                {
                    Thread.Sleep(30000);
                } while (Process.GetProcessById(processId) != null);
            }
            catch
            {
                // if process is no longer running, GetProcessById throws
            }
            string tipText = String.Format("Process {0} has closed. App will close.", this.currentProcessName);
            TrayIcon.ShowBalloonTip(0, Properties.Resources.ApplicationName, tipText, ToolTipIcon.Warning);
            Console.WriteLine(String.Format(tipText));
            Application.Exit();
        }

        private void loadMonitoredProcesses()
        {
            // get list of processes to look for
            MonitoredProcessesConfigurationSection monitoredProcessesSection = ConfigurationManager.GetSection("MonitoredProcessesSection") as MonitoredProcessesConfigurationSection;
            MonitoredProcessCollection monitoredProcesses = monitoredProcessesSection.MonitoredProcesses;
            foreach (MonitoredProcessElement process in monitoredProcesses)
            {
                this.monitoredProcesses.Add(process.Name);
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TrayIcon.Visible = false;
            TrayIcon.Dispose();
        }

    }
}
