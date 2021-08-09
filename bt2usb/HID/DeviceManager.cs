using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.Linux;
using bt2usb.Linux.HID;
using bt2usb.Linux.Udev;
using Mono.Unix.Native;
using Device = bt2usb.Config.Device;

namespace bt2usb.HID
{
    public class DeviceManager: IDisposable
    {
        private readonly Config.Config _config;
        private readonly Dictionary<Device.TypeEnum, DeviceDescriptor> _pipeDictionary;
        private readonly List<Task> _tasks;
        private readonly Listener _listener;
        private readonly Dictionary<string, byte> _gamepadIdDictionary;

        private bool _forwarding;
        private bool _setup;

        public bool IsForwarding => _forwarding;

        private class DeviceDescriptor : IDisposable
        {
            private readonly string _hidRawDevNode;
            private readonly string _hidGadgetDevNode;

            public int HidRawFd { get; private set; }
            public int HidGadgetFd { get; private set; }

            public DeviceDescriptor(string hidRawDevNode, string hidGadgetDevNode)
            {
                _hidRawDevNode = hidRawDevNode;
                _hidGadgetDevNode = hidGadgetDevNode;
                
                HidRawFd = -1;
                HidGadgetFd = -1;
            }
            public void OpenDevNodes()
            {
                HidRawFd = Syscall.open(_hidRawDevNode, OpenFlags.O_RDWR);
                HidGadgetFd = Syscall.open(_hidGadgetDevNode, OpenFlags.O_RDWR); 
            }

            public void Dispose()
            {
                if (HidRawFd >= 0) Syscall.close(HidRawFd);
                if (HidGadgetFd >= 0) Syscall.close(HidGadgetFd);
            }
        }

        public DeviceManager(Config.Config config)
        {
            _config = config;
            _pipeDictionary = new Dictionary<Device.TypeEnum, DeviceDescriptor>();
            _tasks = new List<Task>();
            _listener = new Listener();
            _gamepadIdDictionary = new Dictionary<string, byte>();
        }

        public void Setup()
        {
            if (_setup) return;
            _setup = true;

            _listener.Start();
            
            var deviceDict = new Dictionary<string, Device>();
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
                
                if (!Enum.TryParse<Device.TypeEnum>(deviceDict[uniq].Type, out var type))
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
                
                if (type != Device.TypeEnum.Keyboard && type != Device.TypeEnum.Mouse)
                {
                    byte id;
                    if (_gamepadIdDictionary.ContainsKey(uniq))
                        id = _gamepadIdDictionary[uniq];
                    else
                    {
                        id = (byte)_gamepadIdDictionary.Count;
                        _gamepadIdDictionary.Add(uniq, id);
                    }
                            
                    _listener.AddController(hidRawDevNode, id);
                    continue;
                }
                
                #region Get USB Gadget dev node
                var hidGadgetDevNode = type switch
                {
                    Device.TypeEnum.Keyboard => "/dev/hidg0",
                    Device.TypeEnum.Mouse => "/dev/hidg1",
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
                if (!File.Exists(hidGadgetDevNode))
                {
                    continue;
                }
                #endregion
                
                Console.WriteLine("Building ({2}) descriptor with: {0}, {1}", hidRawDevNode, hidGadgetDevNode, type);
                var descriptor = new DeviceDescriptor(hidRawDevNode, hidGadgetDevNode);
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

        private static unsafe long InputFilter(byte* buf, long len, Device.TypeEnum typeEnum)
        {
            switch (typeEnum)
            {
                case Device.TypeEnum.Keyboard:
                    return len;
                case Device.TypeEnum.Mouse:
                    for (var i = 1; i < len; ++i)
                    {
                        buf[i - 1] = buf[i];
                    }
                    return len - 1;
                case Device.TypeEnum.DS4:
                    return len;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeEnum), typeEnum, null);
            }
        }

        private unsafe void ForwardLoop(DeviceDescriptor descriptor, Device.TypeEnum type)
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