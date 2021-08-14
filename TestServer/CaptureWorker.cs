using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using NAudio.Wave;

namespace TestServer
{
    public class CaptureWorker: IDisposable
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

        private const int BtOutputReportLength = 334;
    
        private readonly byte[] _outputBtCrc32Head = { 0xA2 };
        private readonly NetworkStream _stream;
        private readonly object _syncRoot;
        private readonly byte _id;
        private readonly CircularBuffer<byte> _buffer;
        // private readonly byte[] _newBuffer;
        private readonly SbcEncoder _encoder;
        private bool _capturing;
        private FileStream outputFile;

        public CaptureWorker(NetworkStream stream, object syncRoot, byte id, int sampleRate)
        {
            _stream = stream;
            _syncRoot = syncRoot;
            _id = id;
            _encoder = new SbcEncoder(
                sampleRate,
                8,
                25,
                false,
                true,
                false,
                16
            );
            _buffer = new CircularBuffer<byte>(80000);
            // _newBuffer = new byte[800000];

            outputFile = File.OpenWrite("capture.sbc");
        }

        public void Start()
        {
            _capturing = true;
            Console.WriteLine("Start recording");
        }

        public void LoopbackCaptureOnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!_capturing || e.BytesRecorded <= 0) return;
            
            try
            {
                lock (_buffer)
                {
                    _buffer.CopyFrom(e.Buffer, e.BytesRecorded);
                    // Array.Copy(e.Buffer, 0, _newBuffer, _newBufferOffset, e.BytesRecorded);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
            // Console.WriteLine(e.BytesRecorded);
        }

        public unsafe void Playback()
        {
            Util.timeBeginPeriod(1);

            var minData = (int)_encoder.Codesize * 2 * 2; // 2 frames but at 32 bits
            var audioData = new byte[_buffer.Capacity];
            var s16AudioData = new byte[_buffer.Capacity];
            
            var bufferSize = BtOutputReportLength + 1;
            var outputBuffer = new byte[bufferSize];
            var lilEndianCounter = 0;
            
            var indexBuffer = 81;

            while (_capturing)
            {
                while (_buffer.CurrentLength < minData)
                {
                    Thread.Sleep(1);
                }
                
                lock (_buffer)
                {
                    _buffer.CopyTo(audioData, minData);                    
                }
                
                long encoded, total = 0;
                fixed (byte* s16AudioDataPtr = s16AudioData)
                {
                    fixed (byte* audioDataPtr = audioData)
                    {
                        SampleRate.src_float_to_short_array((float*) audioDataPtr, (short*) s16AudioDataPtr,
                            minData / 2);
                    }
                    
                    fixed (byte* outputBufferPtr = &outputBuffer[indexBuffer])
                    {
                        _encoder.Encode(s16AudioDataPtr, outputBufferPtr, (ulong)minData / 2, out encoded);
                        _encoder.Encode(s16AudioDataPtr + _encoder.Codesize, outputBufferPtr + encoded, (ulong)minData / 2, out encoded);
                    }
                }
                
                if (encoded < 0)
                {
                    Console.WriteLine("Error");
                }
                
                // outputFile.Write(outputBuffer, indexBuffer, (int) total);
                
                // Console.WriteLine(BitConverter.ToString(outputBuffer, indexBuffer, (int) total));
                
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
                
                outputBuffer[78] = (byte)(lilEndianCounter & 255);
                outputBuffer[79] = (byte)((lilEndianCounter / 256) & 255);
                
                //outputBuffer[80] = 0x02; // 0x02 Speaker Mode On / 0x24 Headset Mode On
                outputBuffer[80] = 0x24; // 0x02 Speaker Mode On / 0x24 Headset Mode On
                
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
                
                lock (_syncRoot)
                {
                    _stream.Write(outputBuffer, 0, outputBuffer.Length);
                }
            }
            
            Console.WriteLine("Exited");
        }

        public void StopPlayback()
        {
            _capturing = false;
        }

        public void Flush()
        {
            outputFile.Flush();
        }

        public void Dispose()
        {
            outputFile.Flush();
            outputFile.Dispose();
            _encoder.Dispose();
        }
    }
}