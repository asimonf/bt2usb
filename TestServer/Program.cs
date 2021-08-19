using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FFT.CRC;
using NAudio.Utils;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using NoGcSockets;
using TestServer.Sound;
using Dualshock = TestServer.Hid.Sony.DS4;
using Dualsense = TestServer.Hid.Sony.DualSense;

namespace TestServer
{
    static class Program
    {
        private static readonly object SyncRoot = new object();
        
        private static Socket _socket;
        private static IPEndPoint _socketEndPoint;
        private static bool _running = true;
        
        private static readonly Dictionary<byte, NewCaptureWorker> ControllerWorkers = new Dictionary<byte, NewCaptureWorker>(8);

        private static Thread _forwarderThread;
        
        private static readonly byte[] TouchCounter = new byte[8];
        private static readonly int[] ReportCounter = new int[8];

        private static readonly SbcAudioStream SbcAudioStream = new SbcAudioStream();

        private enum ReportType: byte
        {
            Unknown = 0xFF,
            HID_0x11 = 0x11,
            HID_0x13 = 0x13,
            HID_0x14 = 0x14,
            HID_0x15 = 0x15,
            HID_0x16 = 0x16,
            HID_0x17 = 0x17,
            HID_0x31 = 0x31,
        }
        
        static void Main(string[] args)
        {
            var dict = new ConcurrentDictionary<byte, IDualShock4Controller>();
            using var vigem = new ViGEmClient();
            
            Console.CancelKeyPress += (sender, eventArgs) => _running = false;
            
            while (_running)
            {
                try
                {
                    Setup("192.168.7.2", vigem, dict);
                    Start();
                    while (_forwarderThread.IsAlive)
                    {
                        GC.Collect(GC.MaxGeneration);
                        Thread.Sleep(60 * 1000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }                
            }
        }

        private static void Start()
        {
            SbcAudioStream.Start();
            _forwarderThread.Start();
        }

        private static void Setup(
            string ip,
            ViGEmClient vigem,
            ConcurrentDictionary<byte, IDualShock4Controller> dualShock4Controllers
        )
        {
            _socketEndPoint = new IPEndPoint(IPAddress.Parse("192.168.7.1"), 27000);
            _socket = new Socket(_socketEndPoint.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(_socketEndPoint);

            _forwarderThread = new Thread(() =>
            {
                InputForwarder(vigem, dualShock4Controllers);
            });
        }

        private static unsafe bool FilterPacket(byte id, byte[] buffer, byte[] report, ReportType type)
        {
            switch (type)
            {
                case ReportType.HID_0x11: // Regular DS4 report
                case ReportType.HID_0x14: // Regular DS4 report
                case ReportType.HID_0x15: // Regular DS4 report
                case ReportType.HID_0x16: // Regular DS4 report
                case ReportType.HID_0x17: // Regular DS4 report
                    if ((byte)(buffer[1] & 0x80) > 0) // Has input data
                    {
                        Buffer.BlockCopy(buffer, 3, report, 0, report.Length);
                        return true;
                    }
                    else
                    {
                        // Console.WriteLine("Has no input data");
                        return false;
                    }
                case ReportType.HID_0x31:
                    fixed (void* dualsenseReportPtr = &buffer[3])
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
                                ds4Report->CurrentDs4Touch.PacketCounter = ++TouchCounter[id];
                                ds4Report->CurrentDs4Touch.Finger1 = dualsenseReport->TouchReport.Finger1;
                                ds4Report->CurrentDs4Touch.Finger2 = dualsenseReport->TouchReport.Finger2;
                            }
                            else
                            {
                                ds4Report->PacketNumber = 0;
                                ds4Report->CurrentDs4Touch.PacketCounter = TouchCounter[id];
                                ds4Report->CurrentDs4Touch.Finger1 = dualsenseReport->TouchReport.Finger1;
                                ds4Report->CurrentDs4Touch.Finger2 = dualsenseReport->TouchReport.Finger2;
                            }

                            ds4Report->ExtraTouch1.Finger1.Contact = 0x80;
                            ds4Report->ExtraTouch1.Finger2.Contact = 0x80;
                            ds4Report->ExtraTouch2.Finger1.Contact = 0x80;
                            ds4Report->ExtraTouch2.Finger2.Contact = 0x80;

                            ds4Report->Buttons = dualsenseReport->Buttons;
                            ds4Report->Special = (byte)((dualsenseReport->Special & 0x03) | ((++ReportCounter[id] << 2) & 0xFC));

                            ds4Report->BatteryLevel = (byte) (
                                (dualsenseReport->Status & 0x0f) |
                                ((dualsenseReport->Status & 0xf0) > 0 ? 0x10 : 0x00)
                            );

                            ds4Report->BatteryLevelSpecial = 0x01;
                        }
                    }
                    return true;
                case ReportType.HID_0x13:
                    return false;
                default:
                    Console.WriteLine("Unknown");
                    return false;
            }
        }

        private static void InputForwarder(
            ViGEmClient vigem,
            ConcurrentDictionary<byte,IDualShock4Controller> dualShock4Controllers
        )
        {
            var buffer = new byte[640];
            var report = new byte[63];

            while (true)
            {
                var from = new IPEndPointStruct(new IPHolder(AddressFamily.InterNetwork), 0);
                var len = SocketHandler.ReceiveFrom(_socket, buffer, 0, buffer.Length, SocketFlags.None, ref @from);
                
                if (len <= 0)
                {
                    Console.WriteLine("Error reading");
                    continue;
                }

                var rawReportType = buffer[0];
                
                var reportType = rawReportType switch
                {
                    0x11 => ReportType.HID_0x11,
                    0x13 => ReportType.HID_0x13,
                    0x14 => ReportType.HID_0x14,
                    0x15 => ReportType.HID_0x15,
                    0x16 => ReportType.HID_0x16,
                    0x17 => ReportType.HID_0x17,
                    _ => ReportType.Unknown
                };
                
                var isDs4 = reportType switch
                {
                    ReportType.HID_0x11 => true,
                    ReportType.HID_0x13 => true,
                    ReportType.HID_0x14 => true,
                    ReportType.HID_0x15 => true,
                    ReportType.HID_0x16 => true,
                    ReportType.HID_0x17 => true,
                    _ => false
                };

                var packetSize = len - 1;
                // var micPackets = reportType switch
                // {
                //     ReportType.HID_0x11 => 0,
                //     ReportType.HID_0x13 => 2,
                //     ReportType.HID_0x14 => 2,
                //     ReportType.HID_0x15 => 3,
                //     ReportType.HID_0x16 => 4,
                //     ReportType.HID_0x17 => 5,
                //     ReportType.HID_0x31 => 0,
                //     _ => throw new ArgumentOutOfRangeException("wat")
                // };
                
                unsafe
                {
                    byte btHeader = 0xa1;
                    var crc = CRC32Calculator.SEED;
                    CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(&btHeader, 1));
                    CRC32Calculator.Add(ref crc, new ReadOnlySpan<byte>(buffer, 0, packetSize - 4));
                    crc = CRC32Calculator.Finalize(crc);

                    var currCrc = BitConverter.ToUInt32(buffer, packetSize - 4);
                    
                    if (currCrc != crc)
                    {
                        Console.WriteLine("Crc doesnt match ({4}, {3:x2}): {0:x} {1:x} {2}", currCrc, crc, BitConverter.ToString(buffer, packetSize -4, 4), rawReportType, reportType.ToString());
                        continue;
                    }
                }
                
                var id = buffer[packetSize];
                if (id >= 8)
                {
                    Console.WriteLine("Invalid id (?) - skipping for now");
                    continue;
                }
                
                var hasInputData = FilterPacket(id, buffer, report, reportType);
                
                IDualShock4Controller controller;
                if (!dualShock4Controllers.ContainsKey(id))
                {
                    Console.WriteLine("Creating controller with id: {0}", id);
                    
                    controller = vigem.CreateDualShock4Controller();
                    dualShock4Controllers.TryAdd(id, controller);
                    controller.Connect();

                    if (isDs4)
                    {
                        var worker = new NewCaptureWorker(SbcAudioStream, _socket, SyncRoot, id);
                        controller.FeedbackReceived += (sender, args) =>
                        {
                            worker.SubmitFeedback(args);
                        };

                        ControllerWorkers.TryAdd(id, worker);
                    }
                }
                else
                {
                    controller = dualShock4Controllers[id];
                }
                
                if (hasInputData)
                {
                    controller.SubmitRawReport(report);                    
                }

                Thread.Yield();
            }
        }
        
        private static readonly int[] OutputReportCounter = new int[8];

        private static unsafe void SendDualsenseReport(byte id, DualShock4FeedbackReceivedEventArgs e)
        {
            var size = sizeof(Dualshock.OutputReportBt) + 1;
            var data = stackalloc byte[size];
            Dualsense.OutputReportBt.CreateInstance((Dualsense.OutputReportBt*) data, ++OutputReportCounter[id], e);
            data[size - 1] = id;
            var span = new ReadOnlySpan<byte>(data, size);
            
            Console.WriteLine("Dualsense report: {0}", size);
            
            // _stream.Write(span);
        }
    }
}