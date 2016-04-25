using System;

namespace WinSockHook
{
    public class HookCallbackHandler : MarshalByRefObject, IHookCallbackHandler
    {
        public void OnHookInstalled(string appName, string processName)
        {
            string msg = String.Format("Hook installed for process {0}.", processName);
            Logger.Get(appName).LogInfo(msg);
            Console.WriteLine(msg);
        }

        public void OnHookUninstalled(string appName, string processName)
        {
            string msg = String.Format("Hook uninstalled for process {0}.", processName);
            Logger.Get(appName).LogInfo(msg);
            Console.WriteLine(msg);
        }

        public void OnHookInvocation(string appName, string methodName, string processName)
        {
            string msg = String.Format("Hook invoked for process {0} for method {1}.", processName, methodName);
            Logger.Get(appName).LogInfo(msg);
            Console.WriteLine(msg);
        }

        public void OnErrorCaptured(string appName, string methodName, string processName, int errorCode)
        {
            string msg = String.Format("Error captured process {0} for method {1}. Error code: {2}.", processName, methodName, errorCode);
            Logger.Get(appName).LogInfo(msg);
            Console.WriteLine(msg);
        }

        public void OnError(string appName, string processName, Exception exception)
        {
            string msg = String.Format("An exception was caught for process {0}: {1}.", processName, exception.Message);
            Logger.Get(appName).LogError(msg);
            Console.WriteLine(msg);
        }
    }
}
