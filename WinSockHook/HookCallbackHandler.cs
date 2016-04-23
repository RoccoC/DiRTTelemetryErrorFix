using System;

namespace WinSockHook
{
    public class HookCallbackHandler : MarshalByRefObject, IHookCallbackHandler
    {
        public void OnHookInstalled(string processName)
        {
            // TODO: log
            Console.WriteLine(String.Format("OnHookInstalled for process {0}", processName));
        }

        public void OnHookUninstalled(string processName)
        {
            // TODO: log
            Console.WriteLine(String.Format("OnHookUninstalled for process {0}", processName));
        }

        public void OnHookInvocation(string methodName, string processName)
        {
            // TODO: log
            Console.WriteLine(String.Format("OnHookInvocation for method {0} and process {1}", methodName, processName));
        }

        public void OnErrorCaptured(string methodName, string processName, int errorCode)
        {
            // TODO: log
            Console.WriteLine(String.Format("OnErrorCaptured for method {0} and process {1}; error code: {2}", methodName, processName, errorCode));
        }

        public void OnError(string processName, Exception exception)
        {
            // TODO: log
            Console.WriteLine(String.Format("OnError {0}: {1}", processName, exception.Message));
        }
    }
}
