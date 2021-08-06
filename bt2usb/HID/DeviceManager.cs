using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.Linux;
using bt2usb.Linux.HID;
using bt2usb.Linux.Udev;
using Mono.Unix.Native;

namespace bt2usb.HID
{
    public class DeviceManager: IDisposable
    {
        private readonly Config.Config _config;
        private readonly Dictionary<Config.Device.TypeEnum, DeviceDescriptor> _pipeDictionary;
        private readonly List<Task> _tasks;
        private bool _forwarding = false;

        public bool IsForwarding => _forwarding;

        private class DeviceDescriptor : IDisposable
        {
            private readonly string _inputEventDevNode;
            private readonly string _hidRawDevNode;
            private readonly string _hidGadgetDevNode;

            public int InputEventFd { get; private set; }
            public int HidRawFd { get; private set; }
            public int HidGadgetFd { get; private set; }

            public DeviceDescriptor(string inputEventDevNode, string hidRawDevNode, string hidGadgetDevNode)
            {
                _inputEventDevNode = inputEventDevNode;
                _hidRawDevNode = hidRawDevNode;
                _hidGadgetDevNode = hidGadgetDevNode;

                InputEventFd = -1;
                HidRawFd = -1;
                HidGadgetFd = -1;
            }
            public void OpenDevNodes()
            {
                // InputEventFd = Syscall.open(_inputEventDevNode, OpenFlags.O_RDWR);
                //
                // var res = Tmds.Linux.LibC.ioctl(InputEventFd, (int) InputH.EVIOCGRAB());
                // if (res < 0) throw new Exception($"Unable to get exclusive access on {_inputEventDevNode}");
                
                HidRawFd = Syscall.open(_hidRawDevNode, OpenFlags.O_RDWR);
                HidGadgetFd = Syscall.open(_hidGadgetDevNode, OpenFlags.O_RDWR); 
            }

            public void Dispose()
            {
                if (InputEventFd >= 0) Syscall.close(InputEventFd);
                if (HidRawFd >= 0) Syscall.close(HidRawFd);
                if (HidGadgetFd >= 0) Syscall.close(HidGadgetFd);
            }
        }

        private static void DescribeDevice(Device device)
        {
            Console.WriteLine("--- START ---");

            // try
            // {
            //     Console.WriteLine("Driver {0}", device.Driver);
            // }
            // catch
            // {
            //     Console.WriteLine("No driver");
            // }

            Console.WriteLine("Subsystem {0}", device.Subsystem);
            // Console.WriteLine("DevPath {0}", device.DevPath);

            try
            {
                foreach (var link in device.Tags)
                {
                    Console.WriteLine("Tag {0}", link);
                }
            }
            catch
            {
                Console.WriteLine("No tags");
            }

            try
            {
                foreach (var link in device.AttributeNames)
                {
                    var value = "No value";

                    try
                    {
                        var ret = device.TryGetAttribute(link);

                        value = link == "report_descriptor"
                            ? BitConverter.ToString(ret)
                            : Encoding.ASCII.GetString(ret);
                    }
                    catch
                    {
                        // ignored
                    }

                    if (link == "report_descriptor")
                    {
                    }

                    Console.WriteLine("Attribute {0}: {1}", link, value);
                }
            }
            catch
            {
                Console.WriteLine("No attributes");
            }

            // try
            // {
            //     foreach (var link in device.DevLinks)
            //     {
            //         Console.WriteLine("Tag {0}", link);
            //     }
            // }
            // catch
            // {
            //     Console.WriteLine("No links");
            // }

            Console.WriteLine("DevNode {0}", device.DevNode);
            // Console.WriteLine("SysName {0}", device.SysName);

            foreach (var (key, value) in device.Properties)
            {
                Console.WriteLine("Property {0}: {1}", key, value);
            }

            // Console.WriteLine("SysPath {0}", device.SysPath);
            Console.WriteLine("--- END ---");
            Console.WriteLine();
        }

        public DeviceManager(Config.Config config)
        {
            _config = config;
            _pipeDictionary = new Dictionary<Config.Device.TypeEnum, DeviceDescriptor>();
            _tasks = new List<Task>();
        }

        public void FindDevices()
        {
            var deviceDict = new Dictionary<string, Config.Device>();
            using var context = new Context();
            
            using var hidEnumerator = new Enumerator(context);
            hidEnumerator.AddMatchSubsystem("hid");
            foreach (var configDevice in _config.Devices)
            {
                hidEnumerator.AddMatchProperty("HID_UNIQ", configDevice.Address);
                deviceDict.Add(configDevice.Address, configDevice);
            }

            hidEnumerator.ScanDevices();

            foreach (var device in hidEnumerator)
            {
                #region Get Device Config Type
                var uniq = (
                    from c in device.Properties
                    where c.Key == "HID_UNIQ"
                    select c.Value
                ).Single();
                
                if (!Enum.TryParse<Config.Device.TypeEnum>(deviceDict[uniq].Type, out var type))
                {
                    Console.WriteLine("Error parsing {0}", deviceDict[uniq].Type);
                    continue;
                }
                #endregion

                #region Get HID raw dev node
                string hidRawDevNode;
                using (var hidRawEnumerator = new Enumerator(context))
                {
                    hidRawEnumerator.AddMatchParent(device);
                    hidRawEnumerator.AddMatchSubsystem("hidraw");
                    hidRawEnumerator.ScanDevices();
                    hidRawDevNode = hidRawEnumerator.First().DevNode;
                }
                #endregion
                
                #region Get USB Gadget dev node
                var hidGadgetDevNode = type switch
                {
                    Config.Device.TypeEnum.Keyboard => "/dev/hidg0",
                    Config.Device.TypeEnum.Mouse => "/dev/hidg1",
                    Config.Device.TypeEnum.DS4 => "/dev/hidg2",
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
                if (!File.Exists(hidGadgetDevNode))
                {
                    continue;
                }
                #endregion
                
                #region Get Input dev node
                string inputDevNode = null;
                // using (var enumerator = new Enumerator(context))
                // {
                //     enumerator.AddMatchParent(device);
                //     enumerator.AddMatchSubsystem("input");
                //     enumerator.AddMatchProperty("DEVNAME", "/dev/input/event*");
                //
                //     var property = type switch
                //     {
                //         Config.Device.TypeEnum.Keyboard => "ID_INPUT_KEYBOARD",
                //         Config.Device.TypeEnum.Mouse => "ID_INPUT_MOUSE",
                //         Config.Device.TypeEnum.DS4 => "ID_INPUT_JOYSTICK",
                //         _ => throw new ArgumentOutOfRangeException()
                //     };
                //     
                //     enumerator.ScanDevices();
                //
                //     foreach (var dev in enumerator)
                //     {
                //         var propertyVal = (
                //             from c in dev.Properties
                //             where c.Key == property
                //             select c.Value
                //         ).Single();
                //
                //         if (propertyVal == null) continue;
                //         
                //         inputDevNode = dev.DevNode;
                //         break;
                //     }
                // }
                #endregion

                Console.WriteLine("Building ({2}) descriptor with: {0}, {1}, {3}", hidRawDevNode, hidGadgetDevNode, type, inputDevNode);
                var descriptor = new DeviceDescriptor(inputDevNode, hidRawDevNode, hidGadgetDevNode);
                descriptor.OpenDevNodes();
                _pipeDictionary.Add(type, descriptor);
            }
        }

        public void StartForwarding()
        {
            if (_forwarding) return;
            
            foreach (var (type, descriptor) in _pipeDictionary)
            {
                _forwarding = true;
                _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(descriptor, type)));
            }
        }

        public void StopForwarding()
        {
            if (!_forwarding) return;

            _forwarding = false;

            Task.WaitAll(_tasks.ToArray());
        }

        private static unsafe long InputFilter(byte* buf, long len, Config.Device.TypeEnum typeEnum)
        {
            switch (@typeEnum)
            {
                case Config.Device.TypeEnum.Keyboard:
                    return len;
                case Config.Device.TypeEnum.Mouse:
                    for (var i = 1; i < len; ++i)
                    {
                        buf[i - 1] = buf[i];
                    }
                    return len - 1;
                case Config.Device.TypeEnum.DS4:
                    return len;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeEnum), @typeEnum, null);
            }
        }

        private unsafe void ForwardLoop(DeviceDescriptor descriptor, Config.Device.TypeEnum type)
        {
            var buf = stackalloc byte[32];

            while (_forwarding)
            {
                var fds = new HidHelperH.fd_set();
                HidHelperH.FD_ZERO(&fds);
                HidHelperH.FD_SET(descriptor.HidRawFd, &fds);
                HidHelperH.FD_SET(descriptor.HidGadgetFd, &fds);
                var fdsMax = Math.Max(descriptor.HidRawFd, descriptor.HidGadgetFd);
                var tv = new TimeH.timeval {tv_sec = 1, tv_usec = 0};

                // Console.WriteLine("select {0}", fdsMax);
                
                var ret = HidHelperH.select(fdsMax + 1, ref fds, null, null, &tv);
                if (ret == -1)
                {
                    Console.WriteLine("Error select");
                    return;
                }

                // for (var i = 0; i < 32; i++)
                // {
                //     Console.WriteLine("fds {1} {0}", fds.__fds_bits[i], i);                    
                // }
                
                // Direction: hidRawFd -> hidgFd
                if (HidHelperH.FD_ISSET(descriptor.HidRawFd, &fds))
                {
                    var len = Syscall.read(descriptor.HidRawFd, buf, 32);
                    if (len < 0)
                    {
                        Console.WriteLine("Error read");
                        return;
                    }

                    var bufSpan = new ReadOnlySpan<byte>(buf, (int) len);
                    Console.WriteLine("[in] -> {0}", BitConverter.ToString(bufSpan.ToArray()));

                    len = InputFilter(buf, len, type);

                    if (len > 0)
                    {
                        len = Syscall.write(descriptor.HidGadgetFd, buf, (ulong)len); // Transfer report to hidg device
                        if (len < 0)
                        {
                            Console.WriteLine("Error write");
                            return;
                        }
                    }
                }
                
                // Direction: hidgFd -> hidRawFd
                if (HidHelperH.FD_ISSET(descriptor.HidGadgetFd, &fds))
                {
                    var len = Syscall.read(descriptor.HidGadgetFd, buf, 32);
                    if (len < 0)
                    {
                        Console.WriteLine("Error read");
                        return;
                    }

                    var bufSpan = new ReadOnlySpan<byte>(buf, (int) len);
                    Console.WriteLine("[out] -> {0}", BitConverter.ToString(bufSpan.ToArray()));
                }

                Thread.Yield();
            }
        }

        public void Dispose()
        {
            foreach (var (_, value) in _pipeDictionary)
            {
                value.Dispose();
            }
        }
    }
}