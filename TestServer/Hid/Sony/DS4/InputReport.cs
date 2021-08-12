using System.Runtime.InteropServices;

namespace TestServer.Hid.Sony.DS4
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InputReport
    {
        public byte X;
        public byte Y;
        public byte RX;
        public byte RY;
        public ushort Buttons;
        public byte Special;
        public byte Z;
        public byte RZ;
        public ushort Timestamp;
        public byte BatteryLevel;
        
        public ushort GyroX;
        public ushort GyroY;
        public ushort GyroZ;
        public ushort AccelX;
        public ushort AccelY;
        public ushort AccelZ;
        
        public fixed byte Reserved1[5];
        public byte BatteryLevelSpecial;
        public fixed byte Reserved2[2];

        public byte PacketNumber;
        public TouchReport CurrentDs4Touch;
        public TouchReport ExtraTouch1;
        public TouchReport ExtraTouch2;
    }
}