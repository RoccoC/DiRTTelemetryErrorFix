using DiRTTelemetryErrorFix.Config;
using EasyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WinSockHook;

namespace DiRTTelemetryErrorFix
{
    class TrayIconApplicationContext : ApplicationContext
    {
        private NotifyIcon TrayIcon;
        private List<String> monitoredProcesses = new List<string>();
        private string currentProcessName;
        private int currentProcessId;
        Logger log;
        private const string HOOK_DLL = "WinSockHook.dll";

        public TrayIconApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.log = Logger.Get(Properties.Resources.ApplicationName);

            this.TrayIcon = new NotifyIcon();
            this.TrayIcon.Text = Properties.Resources.ApplicationName;
            this.TrayIcon.Icon = Properties.Resources.TrayIcon;

            // create simple context menu with a "close" menu item
            this.TrayIcon.ContextMenuStrip = new ContextMenuStrip();
            this.TrayIcon.ContextMenuStrip.Items.AddRange(
                new ToolStripItem[] {
                    new ToolStripMenuItem(Properties.Resources.CloseText, null, this.CloseMenuItem_Click)
                }
            );
           
            TrayIcon.Visible = true;

            // start process monitor worker
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(this.Worker_DoWork);
            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, EventArgs e)
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
                TrayIcon.ShowBalloonTip(0, Properties.Resources.ApplicationName, Properties.Resources.NoRunningProcessesMsg, ToolTipIcon.Warning);
                this.log.LogInfo(Properties.Resources.NoRunningProcessesMsg);
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
            string hookAssyPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), HOOK_DLL);
            RemoteHooking.IpcCreateServer<HookCallbackHandler>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);
            RemoteHooking.Inject(processId, InjectionOptions.DoNotRequireStrongName, hookAssyPath, hookAssyPath, new Object[] { channelName, processId, Properties.Resources.ApplicationName });

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

            string tipText = String.Format(Properties.Resources.ProcessClosedMsg, this.currentProcessName);
            TrayIcon.ShowBalloonTip(0, Properties.Resources.ApplicationName, tipText, ToolTipIcon.Info);
            this.log.LogInfo(tipText);
            Application.Exit();
        }

        private void loadMonitoredProcesses()
        {
            // get list of processes to look for from the app config
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
            Application.Exit();
        }

    }
}
