using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace TestServer
{
    public class PlayerWorker
    {
        private const byte Protocol = 0x15;
        private const byte ModeType = 0xC0 | 0x04;
        private const byte TransactionType = 0xA2;
        private const byte FeaturesSwitch = 0xF3;
        private const byte PowerRumbleRight = 0x00;
        private const byte PowerRumbleLeft = 0x00;
        private const byte FlashOn = 0x00;
        private const byte FlashOff = 0x00;
        private const byte VolLeft = 0x48;
        private const byte VolRight = 0x48;
        private const byte VolMic = 0x00;
        private const byte VolSpeaker = 0x90; // Volume Built-in Speaker / 0x4D == Uppercase M (Mute?)
        private const byte LightbarRed = 0xFF;
        private const byte LightbarGreen = 0x00;
        private const byte LightbarBlue = 0xFF;
        
        //protected const int BT_OUTPUT_REPORT_LENGTH = 334;
        protected const int BtOutputReportLength = 334;
    
        private readonly string _filePath;
        private bool _exitWorker = false;
        private readonly byte[] _outputBtCrc32Head = { 0xA2 };
        private readonly NetworkStream _stream;
        private readonly object _syncRoot;
        private readonly byte _id;

        public PlayerWorker(string filePath, NetworkStream stream, object syncRoot, byte id)
        {
            _filePath = filePath;
            _stream = stream;
            _syncRoot = syncRoot;
            _id = id;
        }

        public void Playback()
        {
            Util.timeBeginPeriod(1);

            var testerWatch = new Stopwatch();

            var openedFs = File.OpenRead(_filePath);

            var lilEndianCounter = 0;
            int bytesRead;
            var bufferSize = BtOutputReportLength + 1;
            var outputBuffer = new byte[bufferSize];
            var audioData = new byte[224];
            //while (!exitWorker && (audioData = binReader.ReadBytes(224)).Length > 0)
            testerWatch.Start();
            while (!_exitWorker && (bytesRead = openedFs.Read(audioData, 0, 224)) > 0)
            {
                //Array.Clear(outputBuffer, 0, BT_OUTPUT_REPORT_LENGTH);
                //Console.WriteLine(bytesRead);

                var indexBuffer = 81;
                int indexAudioData;
                if (lilEndianCounter > 0xffff)
                {
                    lilEndianCounter = 0;
                }

                outputBuffer[0] = Protocol;
                outputBuffer[1] = ModeType;
                outputBuffer[2] = TransactionType;
                outputBuffer[3] = FeaturesSwitch;
                outputBuffer[4] = 0x04; // Unknown
                outputBuffer[5] = 0x00;
                outputBuffer[6] = PowerRumbleRight;
                outputBuffer[7] = PowerRumbleLeft;
                outputBuffer[8] = LightbarRed;
                outputBuffer[9] = LightbarGreen;
                outputBuffer[10] = LightbarBlue;
                outputBuffer[11] = FlashOn;
                outputBuffer[12] = FlashOff;
                outputBuffer[13] = 0x00; outputBuffer[14] = 0x00; outputBuffer[15] = 0x00; outputBuffer[16] = 0x00; /* Start Empty Frames */
                outputBuffer[17] = 0x00; outputBuffer[18] = 0x00; outputBuffer[19] = 0x00; outputBuffer[20] = 0x00; /* Start Empty Frames */
                outputBuffer[21] = VolLeft;
                outputBuffer[22] = VolRight;
                outputBuffer[23] = VolMic;
                outputBuffer[24] = VolSpeaker;
                outputBuffer[25] = 0x85;
                //outputBuffer[26] = 0x00; outputBuffer[27] = 0x00; outputBuffer[28] = 0x00; outputBuffer[29] = 0x00; outputBuffer[30] = 0x00; outputBuffer[31] = 0x00; /* Start Empty Frames */
                //outputBuffer[32] = 0x00; outputBuffer[33] = 0x00; outputBuffer[34] = 0x00; outputBuffer[35] = 0x00; outputBuffer[36] = 0x00; outputBuffer[37] = 0x00;
                //outputBuffer[38] = 0x00; outputBuffer[39] = 0x00; outputBuffer[40] = 0x00; outputBuffer[41] = 0x00; outputBuffer[42] = 0x00; outputBuffer[43] = 0x00;
                //outputBuffer[44] = 0x00; outputBuffer[45] = 0x00; outputBuffer[46] = 0x00; outputBuffer[47] = 0x00; outputBuffer[48] = 0x00; outputBuffer[49] = 0x00;
                //outputBuffer[50] = 0x00; outputBuffer[51] = 0x00; outputBuffer[52] = 0x00; outputBuffer[53] = 0x00; outputBuffer[54] = 0x00; outputBuffer[55] = 0x00;
                //outputBuffer[56] = 0x00; outputBuffer[57] = 0x00; outputBuffer[58] = 0x00; outputBuffer[59] = 0x00; outputBuffer[60] = 0x00; outputBuffer[61] = 0x00;
                //outputBuffer[62] = 0x00; outputBuffer[63] = 0x00; outputBuffer[64] = 0x00; outputBuffer[65] = 0x00; outputBuffer[66] = 0x00; outputBuffer[67] = 0x00;
                //outputBuffer[68] = 0x00; outputBuffer[69] = 0x00; outputBuffer[70] = 0x00; outputBuffer[71] = 0x00; outputBuffer[72] = 0x00; outputBuffer[73] = 0x00;
                //outputBuffer[74] = 0x00; outputBuffer[75] = 0x00; outputBuffer[76] = 0x00; outputBuffer[77] = 0x00; /* End Empty Frames */
                outputBuffer[78] = (byte)(lilEndianCounter & 255);
                outputBuffer[79] = (byte)((lilEndianCounter / 256) & 255);
                //outputBuffer[80] = 0x02; // 0x02 Speaker Mode On / 0x24 Headset Mode On
                outputBuffer[80] = 0x24; // 0x02 Speaker Mode On / 0x24 Headset Mode On

                // AUDIO DATA
                for (indexAudioData = 0; indexAudioData < bytesRead; indexAudioData++)
                {
                    outputBuffer[indexBuffer++] = (byte)(audioData[indexAudioData] & 255);
                    //indexBuffer++;
                }

                //outputBuffer[306] = 0x00; outputBuffer[307] = 0x00; outputBuffer[308] = 0x00; outputBuffer[309] = 0x00; outputBuffer[310] = 0x00; outputBuffer[311] = 0x00; /* Start Empty Frames */
                //outputBuffer[312] = 0x00; outputBuffer[313] = 0x00; outputBuffer[314] = 0x00; outputBuffer[315] = 0x00; outputBuffer[316] = 0x00; outputBuffer[317] = 0x00;
                //outputBuffer[318] = 0x00; outputBuffer[319] = 0x00; outputBuffer[320] = 0x00; outputBuffer[321] = 0x00; outputBuffer[322] = 0x00; outputBuffer[323] = 0x00;
                //outputBuffer[324] = 0x00; outputBuffer[325] = 0x00; outputBuffer[326] = 0x00; outputBuffer[327] = 0x00; outputBuffer[328] = 0x00; outputBuffer[329] = 0x00; /* End Empty Frames */
                //outputBuffer[330] = 0x00; outputBuffer[331] = 0x00; outputBuffer[332] = 0x00; outputBuffer[333] = 0x00; /* CRC-32 */

                // Generate CRC-32 data for output buffer and add it to output report
                uint calcCrc32;
                calcCrc32 = ~Crc32Algorithm.Compute(_outputBtCrc32Head);
                calcCrc32 = ~Crc32Algorithm.CalculateBasicHash(ref calcCrc32, ref outputBuffer, 0, BtOutputReportLength-4);

                outputBuffer[330] = (byte)calcCrc32;
                outputBuffer[331] = (byte)(calcCrc32 >> 8);
                outputBuffer[332] = (byte)(calcCrc32 >> 16);
                outputBuffer[333] = (byte)(calcCrc32 >> 24);
                outputBuffer[334] = _id;

                lilEndianCounter += 2;
                //hidDevice.WriteAsyncOutputReportViaInterrupt(outputBuffer);
                //hidDevice.WriteOutputReportViaControl(outputBuffer);

                lock (_syncRoot)
                {
                    _stream.Write(outputBuffer, 0, outputBuffer.Length);
                    // _stream.Write(outputBuffer, 100, 100);
                    // _stream.Write(outputBuffer, 200, 100);
                    // _stream.Write(outputBuffer, 300, 35);
                }

                /*while (testerWatch.Elapsed.TotalMilliseconds < 7)
                {
                    Thread.Sleep(0);`
                }
                */
                Thread.Sleep(4);

                while (testerWatch.Elapsed.TotalMilliseconds < 7.99)//7.9985)
                {
                    Thread.SpinWait(500);
                }

                //Console.WriteLine("ELAPSED TIME");
                //Console.WriteLine(testerWatch.ElapsedMilliseconds);
                testerWatch.Restart();
                //Thread.SpinWait(100000000);
            }

            openedFs.Close();
            //Console.WriteLine("ELAPSED TIME");
            //Console.WriteLine(testerWatch.ElapsedMilliseconds);
            //openFile.Close();

            Util.timeEndPeriod(1);
        }

        public void StopPlayback()
        {
            _exitWorker = true;
        }
    }
}