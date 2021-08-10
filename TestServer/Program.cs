using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using FFT.CRC;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;

namespace TestServer
{
    static class Program
    {
        static void Main(string[] args)
        {
            var dict = new ConcurrentDictionary<byte, IDualShock4Controller>();
            using var vigem = new ViGEmClient();
            
            var receiverEndpoint = new IPEndPoint(IPAddress.Parse("192.168.7.2"), 32100);
            var receiverClient = new TcpClient();
            receiverClient.Connect(receiverEndpoint);
            var receiverStream = receiverClient.GetStream();
            receiverStream.ReadTimeout = -1;
            
            var senderEndpoint = new IPEndPoint(IPAddress.Parse("192.168.7.2"), 32101);
            var senderClient = new TcpClient();
            senderClient.Connect(senderEndpoint);
            var senderStream = senderClient.GetStream();

            var task = Task.Factory.StartNew(() => Forwarder(vigem, receiverStream, senderStream, dict));

            task.Wait();
        }

        private static unsafe void Forwarder(
            ViGEmClient vigem, 
            Stream receiverStream,
            Stream senderStream,
            ConcurrentDictionary<byte, IDualShock4Controller> dualShock4Controllers
        )
        {
            var buffer = stackalloc byte[128];
            var report = new byte[63];

            var buffSpan = new Span<byte>(buffer, 128);
            
            while (true)
            {
                var packetSize = receiverStream.ReadByte();
                if (packetSize != 79) continue;

                buffSpan.Fill(0);
                var bytesReadSoFar = 0;
                while (bytesReadSoFar < packetSize)
                {
                    var bytesRead = receiverStream.Read(buffSpan.Slice(bytesReadSoFar, packetSize - bytesReadSoFar));
                    if (bytesRead <= 0) break;

                    bytesReadSoFar += bytesRead;
                }
                
                if (bytesReadSoFar != packetSize)
                {
                    Console.WriteLine("Houston, we've got a problem... {0}, {1}", packetSize, bytesReadSoFar);
                    continue;
                }
                
                // byte btHeader = 0xa2;
                // var crc = CRC32Calculator.SEED;
                // CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
                // CRC32Calculator.Add(ref crc, buffSpan[..^4]);
                // crc = CRC32Calculator.Finalize(crc);
                //
                // var currentCrc = BitConverter.ToInt32(buffSpan.Slice(buffSpan.Length - 4, 4));
                //
                // if (crc != currentCrc)
                // {
                //     Console.WriteLine("curr: {0}, calc: {1}", currentCrc, crc);
                // }
                
                buffSpan.Slice(3, report.Length).CopyTo(report);
                
                var id = buffer[packetSize - 1];
                if (id != 0)
                {
                    Console.WriteLine("Invalid id (?) - skipping for now");
                    continue;
                }

                IDualShock4Controller controller;
                if (!dualShock4Controllers.ContainsKey(id))
                {
                    Console.WriteLine("Creating controller with id: {0}", id);
                    
                    controller = vigem.CreateDualShock4Controller();
                    dualShock4Controllers.TryAdd(id, controller);
                    controller.Connect();
                    controller.FeedbackReceived += (_, eventArgs) =>
                    {
                        ControllerOnFeedbackReceived(id, eventArgs, senderStream);
                    };
                }
                else
                {
                    controller = dualShock4Controllers[id];
                }
                
                controller.SubmitRawReport(report);
                Task.Yield();
            }
        }

        private static unsafe void ControllerOnFeedbackReceived(
            byte id, 
            DualShock4FeedbackReceivedEventArgs e,
            Stream senderStream
        )
        {
            const int size = 79;
            var data = stackalloc byte[size];
            var offset = 0;

            data[0] = 0x11;
            data[1] = 0xC0;
            data[3] = 0x07;

            offset = 6;

            data[offset++] = e.SmallMotor;
            data[offset++] = e.LargeMotor;

            data[offset++] = e.LightbarColor.Red;
            data[offset++] = e.LightbarColor.Green;
            data[offset] = e.LightbarColor.Blue;

            /* CRC generation */
            byte btHeader = 0xa2;
            var crc = CRC32Calculator.SEED;
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
            CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(data, size - 5));
            crc = CRC32Calculator.Finalize(crc);

            data[size - 5] = (byte)(crc & 0xFF);
            data[size - 4] = (byte)((crc >> 8)  & 0xFF);
            data[size - 3] = (byte)((crc >> 16) & 0xFF);
            data[size - 2] = (byte)((crc >> 24) & 0xFF);
            data[size - 1] = id;

            var buff = new ReadOnlySpan<byte>(data, size);
            senderStream.Write(buff);
        }
    }
}