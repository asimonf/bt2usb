using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using FFT.CRC;
using NAudio.Wave;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Dualshock = TestServer.Hid.Sony.DS4;
using Dualsense = TestServer.Hid.Sony.DualSense;

namespace TestServer
{
    static class Program
    {
        private static readonly object _syncRoot = new object();
        
        private static NetworkStream _stream;
        private static bool _running = true;
        
        private static readonly WasapiLoopbackCapture Capture = new WasapiLoopbackCapture();

        private static Dictionary<byte, (Thread, CaptureWorker)> _playerThreads =
            new Dictionary<byte, (Thread, CaptureWorker)>(); 

        private enum ReportType: byte
        {
            HID_0x11 = 0x11,
            HID_0x14 = 0x14,
            HID_0x16 = 0x16,
            HID_0x31 = 0x31,
        }
        
        static async Task Main(string[] args)
        {
            var dict = new ConcurrentDictionary<byte, IDualShock4Controller>();
            using var vigem = new ViGEmClient();
            Crc32Algorithm.InitializeTable(0xedb88320u);

            Console.CancelKeyPress += (sender, eventArgs) => _running = false;

            Capture.DataAvailable += CaptureOnDataAvailable;
            Capture.StartRecording();
            
            Console.WriteLine(Capture.WaveFormat.Channels);
            Console.WriteLine(Capture.WaveFormat.Encoding);
            Console.WriteLine(Capture.WaveFormat.SampleRate);
            Console.WriteLine(Capture.WaveFormat.BitsPerSample);

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
            
            Capture.StopRecording();
        }

        private static void CaptureOnDataAvailable(object? sender, WaveInEventArgs e)
        {
            foreach (var (_, (_, entry)) in _playerThreads)
            {
                entry.LoopbackCaptureOnDataAvailable(sender, e);
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
        
        private static byte[] _touchCounter = new byte[8];
        private static int[] _reportCounter = new int[8];

        private static unsafe ReportType FilterPacket(byte id, byte[] buffer, byte[] report)
        {
            var reportType = (ReportType) buffer[0];
            
            switch (reportType)
            {
                case ReportType.HID_0x11: // Regular DS4 report
                case ReportType.HID_0x14: // Regular DS4 report
                case ReportType.HID_0x16: // Regular DS4 report
                    Buffer.BlockCopy(buffer, 3, report, 0, report.Length);
                    break;
                case ReportType.HID_0x31:
                    fixed (void* dualsenseReportPtr = &buffer[2])
                    {
                        var dualsenseReport = (Dualsense.InputReport*) dualsenseReportPtr; 
                        fixed (void* ds4ReportPtr = &report[0])
                        {
                            var ds4Report = (Dualshock.InputReport*) ds4ReportPtr;
                            ds4Report->X = dualsenseReport->X;
                            ds4Report->Y = dualsenseReport->Y;
                            ds4Report->RX = dualsenseReport->RX;
                            ds4Report->RY = dualsenseReport->RY;
                            ds4Report->Z = dualsenseReport->Z;
                            ds4Report->RZ = dualsenseReport->RZ;
                            
                            ds4Report->Timestamp = (ushort)((dualsenseReport->SensorTimestamp >> 4) & 0xFFFF);

                            ds4Report->GyroX = dualsenseReport->GyroX;
                            ds4Report->GyroY = dualsenseReport->GyroY;
                            ds4Report->GyroZ = dualsenseReport->GyroZ;

                            ds4Report->AccelX = dualsenseReport->AccelX;
                            ds4Report->AccelY = dualsenseReport->AccelY;
                            ds4Report->AccelZ = dualsenseReport->AccelZ;

                            if (
                                (dualsenseReport->TouchReport.Finger1.Contact & 0x80) == 0 ||
                                (dualsenseReport->TouchReport.Finger2.Contact & 0x80) == 0
                            )
                            {
                                ds4Report->PacketNumber = 1;
                                ds4Report->CurrentDs4Touch.PacketCounter = ++_touchCounter[id];
                                ds4Report->CurrentDs4Touch.Finger1 = dualsenseReport->TouchReport.Finger1;
                                ds4Report->CurrentDs4Touch.Finger2 = dualsenseReport->TouchReport.Finger2;
                            }
                            else
                            {
                                ds4Report->PacketNumber = 0;
                                ds4Report->CurrentDs4Touch.PacketCounter = _touchCounter[id];
                                ds4Report->CurrentDs4Touch.Finger1 = dualsenseReport->TouchReport.Finger1;
                                ds4Report->CurrentDs4Touch.Finger2 = dualsenseReport->TouchReport.Finger2;
                            }

                            ds4Report->ExtraTouch1.Finger1.Contact = 0x80;
                            ds4Report->ExtraTouch1.Finger2.Contact = 0x80;
                            ds4Report->ExtraTouch2.Finger1.Contact = 0x80;
                            ds4Report->ExtraTouch2.Finger2.Contact = 0x80;

                            ds4Report->Buttons = dualsenseReport->Buttons;
                            ds4Report->Special = (byte)((dualsenseReport->Special & 0x03) | ((++_reportCounter[id] << 2) & 0xFC));

                            ds4Report->BatteryLevel = (byte) (
                                (dualsenseReport->Status & 0x0f) |
                                ((dualsenseReport->Status & 0xf0) > 0 ? 0x10 : 0x00)
                            );

                            ds4Report->BatteryLevelSpecial = 0x01;
                        }
                    }
                    break;
            }

            return reportType;
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
                
                var packetSize = 79;

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
                
                var id = buffer[packetSize - 1];
                if (id >= 8)
                {
                    Console.WriteLine("Invalid id (?) - skipping for now");
                    continue;
                }
                
                var reportType = FilterPacket(id, buffer, report);
                var isDs4 = reportType switch
                {
                    ReportType.HID_0x11 => true,
                    ReportType.HID_0x14 => true,
                    ReportType.HID_0x16 => true,
                    _ => false
                };

                // Console.WriteLine("packet: {0}", BitConverter.ToString(report));

                IDualShock4Controller controller;
                if (!dualShock4Controllers.ContainsKey(id))
                {
                    Console.WriteLine("Creating controller with id: {0}", id);
                    
                    controller = vigem.CreateDualShock4Controller();
                    dualShock4Controllers.TryAdd(id, controller);
                    controller.Connect();
                    // controller.FeedbackReceived += (state, eventArgs) =>
                    // {
                    //     try
                    //     {
                    //         lock (_syncRoot)
                    //         {
                    //             ControllerOnFeedbackReceived(id, reportType, state as IDualShock4Controller, eventArgs);                                
                    //         }
                    //     }
                    //     catch
                    //     {
                    //         controller.Disconnect();
                    //         dualShock4Controllers.TryRemove(id, out _);
                    //     }
                    // };

                    if (isDs4)
                    {
                        var worker = new CaptureWorker(_stream, _syncRoot, id, Capture.WaveFormat.SampleRate);
                        var playerThread = new Thread(() =>
                        {
                            SendDs4Report(id, new DualShock4FeedbackReceivedEventArgs(0, 0, new LightbarColor(55, 0, 55)));
                            Thread.Sleep(500);
                            worker.Start();
                            worker.Playback();
                        });
                        playerThread.IsBackground = true;
                        playerThread.Priority = ThreadPriority.AboveNormal;
                        
                        if (_playerThreads.TryAdd(id, (playerThread, worker)))
                        {
                            playerThread.Start();                        
                        }
                    }
                }
                else
                {
                    controller = dualShock4Controllers[id];
                }
                
                controller.SubmitRawReport(report);
                await Task.Yield();
            }
        }
        
        private static int[] _outputReportCounter = new int[8];

        private static void ControllerOnFeedbackReceived(byte id,
            ReportType reportType,
            IDualShock4Controller controller,
            DualShock4FeedbackReceivedEventArgs e)
        {
            Console.Write(
                "Feedback for {5} ({6}) -> small: {0}, large: {1}, red: {2}, green, {3}, blue: {4} -- ",
                e.SmallMotor,
                e.LargeMotor,
                e.LightbarColor.Red,
                e.LightbarColor.Green,
                e.LightbarColor.Blue,
                id,
                reportType
            );
            
            switch (reportType)
            {
                case ReportType.HID_0x11:
                    SendDs4Report(id, e);
                    break;
                case ReportType.HID_0x31:
                    SendDualsenseReport(id, e);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reportType), reportType, null);
            }
        }

        private static unsafe void SendDualsenseReport(byte id, DualShock4FeedbackReceivedEventArgs e)
        {
            var size = sizeof(Dualshock.OutputReportBt) + 1;
            var data = stackalloc byte[size];
            Dualsense.OutputReportBt.CreateInstance((Dualsense.OutputReportBt*) data, ++_outputReportCounter[id], e);
            data[size - 1] = id;
            var span = new ReadOnlySpan<byte>(data, size);
            
            Console.WriteLine("Dualsense report: {0}", size);
            
            _stream.Write(span);
        }

        private static unsafe void SendDs4Report(byte id, DualShock4FeedbackReceivedEventArgs e)
        {
            var size = sizeof(Dualshock.OutputReportBt) + 1;
            var data = stackalloc byte[size];
            Dualshock.OutputReportBt.CreateInstance((Dualshock.OutputReportBt*) data, e);
            data[size - 1] = id;
            var span = new ReadOnlySpan<byte>(data, size);
            
            Console.WriteLine("Dualshock4 report: {0}", size);
            
            lock (_syncRoot)
                _stream.Write(span);
        }
    }
}