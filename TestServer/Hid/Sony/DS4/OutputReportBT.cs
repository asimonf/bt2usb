using System;
using System.Runtime.InteropServices;
using FFT.CRC;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace TestServer.Hid.Sony.DS4
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OutputReportBt
    {
        public byte ReportId;
        public byte Reserved1;
        public byte Reserved2;
        public byte Reserved3;
        
        public fixed byte Unknown1[2];
        
        public byte SmallMotor;
        public byte LargeMotor;
        
        public byte LightbarRed;
        public byte LightbarGreen;
        public byte LightbarBlue;
        
        public fixed byte Unknown2[63];
        
        public uint Crc32;

        public static void CreateInstance(
            OutputReportBt* report,
            DualShock4FeedbackReceivedEventArgs eventArgs
        )
        {
            report->ReportId = 0x11;
            report->Reserved1 = 0xC0;
            report->Reserved3 = 0x07;
            report->SmallMotor = eventArgs.SmallMotor;
            report->LargeMotor = eventArgs.LargeMotor;
            report->LightbarRed = eventArgs.LightbarColor.Red;
            report->LightbarGreen = eventArgs.LightbarColor.Green;
            report->LightbarBlue = eventArgs.LightbarColor.Blue;
            
            /* CRC generation */
            byte btHeader = 0xa2;
            var crc = CRC32Calculator.SEED;
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(report, sizeof(OutputReportBt) - 4));
            report->Crc32 = CRC32Calculator.Finalize(crc);
        }
    }
}