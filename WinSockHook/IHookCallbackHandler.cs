using System;

namespace WinSockHook
{
    public interface IHookCallbackHandler
    {
        void OnError(string processName, Exception exception);
        void OnErrorCaptured(string methodName, string processName, int errorCode);
        void OnHookInstalled(string processName);
        void OnHookUninstalled(string processName);
        void OnHookInvocation(string methodName, string processName);
    }
}