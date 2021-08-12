using System.Runtime.InteropServices;

namespace TestServer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TouchReport
    {
        public byte PacketCounter;

        public TouchFinger Finger1;
        public TouchFinger Finger2;
    }
}