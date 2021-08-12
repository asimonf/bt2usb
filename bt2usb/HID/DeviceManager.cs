using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.Linux.Udev;
using bt2usb.Server.Model;
using Newtonsoft.Json;

namespace bt2usb.HID
{
    public class DeviceManager : IDisposable
    {
        private readonly Context _udevContext;
        private readonly HidForwarder _hidForwarder;
        private readonly GamepadForwarder _gamepadForwarder;

        public DeviceManager()
        {
            _udevContext = new Context();
            _hidForwarder = new HidForwarder();
            _gamepadForwarder = new GamepadForwarder();
        }

        public void Setup()
        {
            _gamepadForwarder.Start();
        }

        public void NewBtMap(string address, BtDeviceType type, bool connected)
        {
            Console.WriteLine("New BT event received for {0} -> {1} ({2})", address, type, connected ? "Connected" : "Disconnected");
            
            if (connected)
            {
                Console.Write("Settling...");
                Thread.Sleep(1000);
                Console.WriteLine("Done");
                
                using var hidEnumerator = new Enumerator(_udevContext);
                hidEnumerator.AddMatchSubsystem("hid");
                hidEnumerator.ScanDevices();

                foreach (var device in hidEnumerator)
                {
                    var uniq = (
                        from c in device.Properties
                        where c.Key == "HID_UNIQ"
                        select c.Value
                    ).SingleOrDefault()?.ToUpperInvariant();

                    if (string.IsNullOrEmpty(uniq)) continue;

                    if (uniq != address) continue;

                    Console.WriteLine("Found device");
                    ProcessDevice(uniq, type, device);
                }
            }
            else
            {
                if (type == BtDeviceType.Keyboard || type == BtDeviceType.Mouse)
                {
                    Console.WriteLine("Stopping forwarder for {0} due to disconnection", address);
                    _hidForwarder.DeleteDescriptor(address);
                }
                else
                {
                    _gamepadForwarder.DeleteDeviceMap(address);
                }
            }
            
        }

        public void Dispose()
        {
            _gamepadForwarder.Dispose();
            _hidForwarder.Dispose();
            _udevContext.Dispose();
        }

        private void ProcessDevice(string uniq, BtDeviceType type, Device device)
        {
            #region Get HID raw dev node

            string hidRawDevNode;
            using (var hidRawEnumerator = new Enumerator(_udevContext))
            {
                hidRawEnumerator.AddMatchParent(device);
                hidRawEnumerator.AddMatchSubsystem("hidraw");
                hidRawEnumerator.ScanDevices();
                hidRawDevNode = hidRawEnumerator.First().DevNode;
            }

            #endregion

            if (type != BtDeviceType.Keyboard && type != BtDeviceType.Mouse)
            {
                Console.WriteLine("Adding controller");
                _gamepadForwarder.AddDeviceMap(uniq, hidRawDevNode);
                return;
            }

            #region Get USB Gadget dev node

            var hidGadgetDevNode = type switch
            {
                BtDeviceType.Keyboard => "/dev/hidg0",
                BtDeviceType.Mouse => "/dev/hidg1",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            if (!File.Exists(hidGadgetDevNode))
            {
                Console.WriteLine("Gadgets not properly configured!!");
                return;
            }

            #endregion

            Console.WriteLine("Building ({2}) descriptor with: {0}, {1}", hidRawDevNode, hidGadgetDevNode, type);
            var descriptor = new DeviceDescriptor(hidRawDevNode, hidGadgetDevNode);
            descriptor.OpenDevNodes();
            _hidForwarder.AddDescriptor(uniq, descriptor, type);
        }
    }
}