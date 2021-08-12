using System.Runtime.InteropServices;

namespace TestServer.Hid.Sony.DualSense
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TouchReport
    {
        public TouchFinger Finger1;
        public TouchFinger Finger2;
    }
}