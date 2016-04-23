using EasyHook;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using WinSockHook.WinSockTypes;

namespace WinSockHook
{
    public class Main : IEntryPoint, IDisposable
    {
        private IHookCallbackHandler handler;
        private LocalHook sendToLocalHook;
        private string processName;

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int sendto(IntPtr Socket, IntPtr buff, int len, SendDataFlags flags, ref SockAddr To, int tomlen);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern void WSASetLastError(int set);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        public delegate int SendToDelegate(IntPtr Socket, IntPtr buff, int len, SendDataFlags flags, ref SockAddr To, int tomlen);


        public Main(RemoteHooking.IContext context, string channelName, int processId)
        {
            try
            {
                this.handler = RemoteHooking.IpcConnectClient<HookCallbackHandler>(channelName);
                this.processName = Process.GetProcessById(processId).ProcessName;
            }
            catch (Exception exception)
            {
                this.handler.OnError(this.processName, exception);
            }
        } 

        public void Run(RemoteHooking.IContext context, string channelName, int processId)
        {
            try
            {
                this.sendToLocalHook = LocalHook.Create(LocalHook.GetProcAddress("ws2_32.dll", "sendto"), new SendToDelegate(this.SendToHook), this);
                this.sendToLocalHook.ThreadACL.SetExclusiveACL(new int[] { 0 });
                this.handler.OnHookInstalled(this.processName);
            }
            catch (Exception exception)
            {
                this.handler.OnError(this.processName, exception);
            }

            // wait for host process termination
            try
            {
                do
                {
                    Thread.Sleep(30000);
                } while (Process.GetProcessById(processId) != null);
                this.Dispose();
            }
            catch (Exception exception)
            {
                this.handler.OnError(this.processName, exception);
            }
        }

        /*
         * This is the winsock hook which intercepts the "sendto" method.
         */
        private int SendToHook(IntPtr Socket, IntPtr buff, int len, SendDataFlags flags, ref SockAddr To, int tomlen)
        {
            int returnCode = 0;
            try
            {
                returnCode = sendto(Socket, buff, len, flags, ref To, tomlen);
                if (returnCode == -1) {
                    int errCode = Marshal.GetLastWin32Error();
                    if (errCode == (int)WSA_ERROR.WSAENOTSOCK) {
                        // swallow the original eeror and spoof a good return code
                        this.handler.OnErrorCaptured("sendto", this.processName, errCode);
                        WSASetLastError(0);
                        returnCode = 0;
                    }
                }
            }
            catch (Exception exception)
            {
                this.handler.OnError(this.processName, exception);
            }

            return returnCode;
        }

        public void Dispose()
        {
            this.sendToLocalHook.Dispose();
            this.handler.OnHookUninstalled(this.processName);
        }
    }
}
