using System;
using System.Runtime.InteropServices;

namespace WinSockHook.WinSockTypes
{
    [Flags]
    public enum SendDataFlags
    {
        None = 0,
        DontRoute = 1,
        OOB = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SockAddr
    {
        public short Family;
        public ushort Port;
        public AddressIP4 IPAddress;
        private Int64 Zero;

        public SockAddr(short Family, ushort Port, AddressIP4 IP)
        {
            this.Family = Family;
            this.Port = Port;
            this.IPAddress = IP;
            this.Zero = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AddressIP4
    {
        public byte a1;
        public byte a2;
        public byte a3;
        public byte a4;
        public static AddressIP4 Broadcast { get { return new AddressIP4(255, 255, 255, 255); } }
        public static AddressIP4 AnyAddress { get { return new AddressIP4(0, 0, 0, 0); } }
        public static AddressIP4 Loopback { get { return new AddressIP4(127, 0, 0, 1); } }

        public AddressIP4(byte a1, byte a2, byte a3, byte a4)
        {
            this.a1 = a1;
            this.a2 = a2;
            this.a3 = a3;
            this.a4 = a4;
        }
    }

    public enum WSA_ERROR
    {
        WSAENOTSOCK = 10038
    }
}
