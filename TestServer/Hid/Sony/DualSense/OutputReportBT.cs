using System;
using System.Runtime.InteropServices;
using FFT.CRC;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace TestServer.Hid.Sony.DualSense
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OutputReportBt
    {
        public byte ReportId;
        public byte SequenceTag;
        public byte Tag;
        public OutputReportCommon Common;
        public fixed byte Reserved[24];
        public uint Crc32;

        public static void CreateInstance(
            OutputReportBt* report,
            int counter,
            DualShock4FeedbackReceivedEventArgs eventArgs
        )
        {
            report->ReportId = 0x31;
            report->SequenceTag = (byte) ((counter << 4) & 0xf0);
            report->Tag = 0x10;
            report->Common.ValidFlag0 = Flag0.CompatibleVibration | Flag0.HapticsSelect;
            report->Common.ValidFlag1 = Flag1.LightbarControlEnable;
            report->Common.MotorRight = eventArgs.SmallMotor;
            report->Common.MotorLeft = eventArgs.LargeMotor;
            report->Common.LightbarRed = eventArgs.LightbarColor.Red;
            report->Common.LightbarGreen = eventArgs.LightbarColor.Green;
            report->Common.LightbarBlue = eventArgs.LightbarColor.Red;
            
            /* CRC generation */
            byte btHeader = 0xa2;
            var crc = CRC32Calculator.SEED;
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(report, sizeof(OutputReportBt) - 4));
            report->Crc32 = CRC32Calculator.Finalize(crc);
        }
    }
}