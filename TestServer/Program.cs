using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static object _syncRoot = new object();
        private static NetworkStream _stream;
        private static bool _running = true;
        
        static async Task Main(string[] args)
        {
            var dict = new ConcurrentDictionary<byte, IDualShock4Controller>();
            using var vigem = new ViGEmClient();

            Console.CancelKeyPress += (sender, eventArgs) => _running = false; 

            while (_running)
            {
                try
                {
                    await Setup("192.168.7.2", vigem, dict);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }
        }

        private static async Task Setup(
            string ip,
            ViGEmClient vigem,
            ConcurrentDictionary<byte, IDualShock4Controller> dualShock4Controllers
        )
        {
            using var client = new TcpClient {LingerState = {Enabled = false}};

            await client.ConnectAsync(IPAddress.Parse(ip), 32100);
            
            lock (_syncRoot)
                _stream = client.GetStream();

            await Forwarder(vigem, dualShock4Controllers);
            
            client.Close();
        }

        private static async Task Forwarder(
            ViGEmClient vigem,
            ConcurrentDictionary<byte,IDualShock4Controller> dualShock4Controllers
        )
        {
            var buffer = new byte[128];
            var report = new byte[63];

            while (true)
            {
                if (!_stream.DataAvailable) continue;
                
                var resp = await _stream.ReadAsync(buffer, 0, 1);
                if (resp <= 0)
                {
                    throw new Exception("Error reading from receiver");
                }
                
                var packetSize = buffer[0];
                if (packetSize != 79) continue;

                Array.Fill<byte>(buffer, 0);

                var bytesReadSoFar = 0;
                while (bytesReadSoFar < packetSize)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, bytesReadSoFar, packetSize - bytesReadSoFar);
                    if (bytesRead <= 0) break;

                    bytesReadSoFar += bytesRead;
                }
                
                if (bytesReadSoFar != packetSize)
                {
                    Console.WriteLine("Houston, we've got a problem... {0}, {1}", packetSize, bytesReadSoFar);
                    continue;
                }
                
                Buffer.BlockCopy(buffer, 3, report, 0, report.Length);
                
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
                    controller.FeedbackReceived += (state, eventArgs) =>
                    {
                        try
                        {
                            ControllerOnFeedbackReceived(id, state as IDualShock4Controller, eventArgs);
                        }
                        catch
                        {
                            controller.Disconnect();
                            dualShock4Controllers.TryRemove(id, out _);
                        }
                    };
                }
                else
                {
                    controller = dualShock4Controllers[id];
                }
                
                controller.SubmitRawReport(report);
                await Task.Yield();
            }
        }

        private static unsafe void ControllerOnFeedbackReceived(
            byte id, 
            IDualShock4Controller controller,
            DualShock4FeedbackReceivedEventArgs e
        )
        {
            const int size = 79;
            var data = stackalloc byte[size];

            data[0] = 0x11;
            data[1] = 0xC0;
            data[3] = 0x07;

            var offset = 6;

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

            lock (_syncRoot)
                _stream.Write(buff);
        }
    }
}