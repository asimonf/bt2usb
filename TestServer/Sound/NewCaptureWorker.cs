using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FFT.CRC;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using NoGcSockets;

namespace TestServer.Sound
{
    public class NewCaptureWorker
    {
        // private const byte Frames = 2;
        // private const byte Frames = 4;
        // private const byte Protocol = 0x15;
        // private const byte Protocol = 0x19;
        // private const int BtOutputReportLength = 334;
        // private const int BtOutputReportLength = 547;
        
        private const byte ModeType = 0xC0 | 0x04;
        private const byte TransactionType = 0xA2;
        private const byte FeaturesSwitch = 0xF3;
        private const byte FlashOn = 0x00;
        private const byte FlashOff = 0x00;
        private const byte VolLeft = 0x48;
        private const byte VolRight = 0x48;
        private const byte VolMic = 0x00;
        private const byte VolSpeaker = 0x90; // Volume Built-in Speaker / 0x4D == Uppercase M (Mute?)

        private byte _powerRumbleWeak;
        private byte _powerRumbleStrong;
        private byte _lightbarRed = 0xFF;
        private byte _lightbarGreen;
        private byte _lightbarBlue = 0xFF;

        private readonly Socket _socket;
        private readonly object _syncRoot;
        private readonly byte _id;

        private readonly byte[] _outputBuffer = new byte[640];
        private ushort _lilEndianCounter = 0;

        private IPEndPointStruct _sendTarget  = new IPEndPointStruct(new IPHolder(IPAddress.Parse("192.168.7.2")), 27000);

        public NewCaptureWorker(
            SbcAudioStream audioStream, 
            Socket socket, 
            object syncRoot, 
            byte id
        ) {
            _socket = socket;
            _syncRoot = syncRoot;
            _id = id;

            audioStream.SbcFramesAvailable += AudioStreamOnSbcFramesAvailable;
        }

        private unsafe void AudioStreamOnSbcFramesAvailable(byte[] data, int framesAvailable, int dataLength)
        {
            int protocol, size;
            
            switch (framesAvailable)
            {
                case 4:
                    protocol = 0x19;
                    size = 547;
                    break;
                case 2:
                    protocol = 0x15;
                    size = 334;
                    break;
                default:
                    return;
            }
            
            Array.Fill<byte>(_outputBuffer, 0);
            
            _outputBuffer[0] = (byte) protocol;
            _outputBuffer[1] = ModeType;
            _outputBuffer[2] = TransactionType;
            _outputBuffer[3] = FeaturesSwitch;
            _outputBuffer[4] = 0x04; // Unknown
            _outputBuffer[5] = 0x00;
            _outputBuffer[6] = _powerRumbleWeak;
            _outputBuffer[7] = _powerRumbleStrong;
            _outputBuffer[8] = _lightbarRed;
            _outputBuffer[9] = _lightbarGreen;
            _outputBuffer[10] = _lightbarBlue;
            _outputBuffer[11] = FlashOn;
            _outputBuffer[12] = FlashOff;
            
            _outputBuffer[21] = VolLeft;
            _outputBuffer[22] = VolRight;
            _outputBuffer[23] = VolMic;
            _outputBuffer[24] = VolSpeaker;
            _outputBuffer[25] = 0x85;
            
            _outputBuffer[78] = (byte) (_lilEndianCounter & 0xFF);
            _outputBuffer[79] = (byte) ((_lilEndianCounter >> 8) & 0xFF);
            
            // _outputBuffer[80] = 0x02; // 0x02 Speaker Mode On / 0x24 Headset Mode On
            _outputBuffer[80] = 0x24; // 0x02 Speaker Mode On / 0x24 Headset Mode On

            _lilEndianCounter += (ushort) framesAvailable;
            
            Buffer.BlockCopy(data, 0, _outputBuffer, 81, dataLength);
            
            var crc = CRC32Calculator.SEED;
            byte btHeader = 0xa2;
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(_outputBuffer, 0, size - 4));
            crc = CRC32Calculator.Finalize(crc);
            
            _outputBuffer[size - 4] = (byte) crc;
            _outputBuffer[size - 3] = (byte) (crc >> 8);
            _outputBuffer[size - 2] = (byte) (crc >> 16);
            _outputBuffer[size - 1] = (byte) (crc >> 24);
            _outputBuffer[size] = _id;
            
            lock (_syncRoot)
                SocketHandler.SendTo(_socket, _outputBuffer, 0, size + 1, 0, ref _sendTarget);
        }

        public void SubmitFeedback(DualShock4FeedbackReceivedEventArgs args)
        {
            _powerRumbleStrong = args.SmallMotor;
            _powerRumbleWeak = args.LargeMotor;
            _lightbarBlue = args.LightbarColor.Blue;
            _lightbarGreen = args.LightbarColor.Green;
            _lightbarRed = args.LightbarColor.Red;
        }
    }
}