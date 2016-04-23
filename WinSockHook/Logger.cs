using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinSockHook
{
    public class Logger
    {
        private static Logger instance;

        private string appName;
        private Logger(string appName) {
            this.appName = appName;
            if (!EventLog.SourceExists(appName))
            {
                EventLog.CreateEventSource(appName, "Application");
            }
        }

        public static Logger Get(string appName)
        {
            if (Logger.instance == null)
            {
                Logger.instance = new Logger(appName);
            }
            return Logger.instance;
        }

        public void LogInfo(string msg)
        {
           this.log(msg, EventLogEntryType.Information);
        }

        public void LogError(string msg)
        {
            this.log(msg, EventLogEntryType.Error);
        }

        private void log(string msg, EventLogEntryType type)
        {
            EventLog log = new EventLog();
            log.Source = this.appName;
            log.WriteEntry(msg, type);
        }
    }
}
