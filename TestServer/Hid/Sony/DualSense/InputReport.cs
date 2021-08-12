using System.Runtime.InteropServices;

namespace TestServer.Hid.Sony.DualSense
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct InputReport
    {
        public byte X;
        public byte Y;
        public byte RX;
        public byte RY;
        public byte Z;
        public byte RZ;
        public byte SequenceNumber;
        public ushort Buttons;
        public byte Special;
        public fixed byte Reserved1[5];
        
        public ushort GyroX;
        public ushort GyroY;
        public ushort GyroZ;
        public ushort AccelX;
        public ushort AccelY;
        public ushort AccelZ;
        public uint SensorTimestamp;
        public byte Reserved2;

        public TouchReport TouchReport;
        
        public fixed byte Reserved3[12];
        public byte Status;
        public fixed byte Reserved4[10];
    }
}