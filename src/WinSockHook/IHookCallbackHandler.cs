using System;

namespace WinSockHook
{
    public interface IHookCallbackHandler
    {
        void OnError(string appName, string processName, Exception exception);
        void OnErrorCaptured(string appName, string methodName, string processName, int errorCode);
        void OnHookInstalled(string appName, string processName);
        void OnHookUninstalled(string appName, string processName);
        void OnHookInvocation(string appName, string methodName, string processName);
    }
}