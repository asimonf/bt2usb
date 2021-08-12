using System;
using System.Runtime.InteropServices;

namespace TestServer.Hid.Sony.DualSense
{
    [Flags]
    public enum Flag0: byte
    {
        None = 0,
        CompatibleVibration = 1 << 0,
        HapticsSelect = 1 << 1,
    }
    
    [Flags]
    public enum Flag1: byte
    {
        None = 0,
        MicMuteLedControlEnable = 1 << 0,
        PowerSaveControlEnable = 1 << 1,
        LightbarControlEnable = 1 << 2,
        ReleaseLeds = 1 << 3,
        PlayerIndicatorControlEnable = 1 << 4,
    }

    public enum Flag2 : byte
    {
        None = 0,
        LightbarSetupControlEnable = 1 << 1
    }
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OutputReportCommon
    {
        public Flag0 ValidFlag0;
        public Flag1 ValidFlag1;

        /* For DualShock 4 compatibility mode. */
        public byte MotorRight;
        public byte MotorLeft;

        /* Audio controls */
        public fixed byte Reserved[4];
        public byte MuteButtonLed;

        public byte PowerSaveControl;
        public fixed byte Reserved2[28];

        /* LEDs and lightbar */
        public Flag2 ValidFlag2;
        public fixed byte Reserved3[2];
        public byte LightBarSetup;
        public byte LedBrightness;
        public byte PlayerLeds;
        public byte LightbarRed;
        public byte LightbarGreen;
        public byte LightbarBlue;
    }
}