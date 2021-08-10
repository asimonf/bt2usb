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
using Monitor = bt2usb.Linux.Udev.Monitor;

namespace bt2usb.HID
{
    public class DeviceManager : IDisposable
    {
        private readonly Config.Config _config;
        private readonly Dictionary<string, Device> _deviceDict = new Dictionary<string, Device>();
        private readonly Dictionary<string, byte> _gamepadIdDictionary = new Dictionary<string, byte>();
        private readonly Listener _listener = new Listener();
        private readonly Monitor _monitor;

        private readonly Dictionary<Device.TypeEnum, DeviceDescriptor> _pipeDictionary =
            new Dictionary<Device.TypeEnum, DeviceDescriptor>();

        private readonly List<Task> _tasks = new List<Task>();

        private readonly Context _udevContext = new Context();

        private bool _setup;

        public DeviceManager(Config.Config config)
        {
            _config = config;
            _monitor = new Monitor(_udevContext);
            _monitor.AddMatchSubsystem("hid");
        }

        public bool IsForwarding { get; private set; }

        public void Dispose()
        {
            foreach (var (_, value) in _pipeDictionary) value.Dispose();

            _listener.Stop();

            _monitor.Dispose();
            _udevContext.Dispose();
        }

        public void Setup()
        {
            if (_setup) return;

            _listener.Start();

            using var hidEnumerator = new Enumerator(_udevContext);
            hidEnumerator.AddMatchSubsystem("hid");
            foreach (var configDevice in _config.Devices)
            {
                hidEnumerator.AddMatchProperty("HID_UNIQ", configDevice.Address);
                _deviceDict.Add(configDevice.Address, configDevice);
            }

            hidEnumerator.ScanDevices();

            foreach (var device in hidEnumerator)
            {
                var uniq = (
                    from c in device.Properties
                    where c.Key == "HID_UNIQ"
                    select c.Value
                ).SingleOrDefault();

                if (string.IsNullOrEmpty(uniq)) continue;

                ProcessDevice(uniq, device);
            }

            _setup = true;
        }

        private void ProcessDevice(string uniq, Linux.Udev.Device device)
        {
            #region Get Device Config Type

            if (!Enum.TryParse<Device.TypeEnum>(_deviceDict[uniq].Type, out var type))
            {
                Console.WriteLine("Error parsing {0}", _deviceDict[uniq].Type);
                return;
            }

            #endregion

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

            if (type != Device.TypeEnum.Keyboard && type != Device.TypeEnum.Mouse)
            {
                byte id;
                if (_gamepadIdDictionary.ContainsKey(uniq))
                {
                    id = _gamepadIdDictionary[uniq];
                }
                else
                {
                    id = (byte) _gamepadIdDictionary.Count;
                    _gamepadIdDictionary.Add(uniq, id);
                }

                _listener.AddController(hidRawDevNode, id);
                return;
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
                Console.WriteLine("Gadgets not properly configured!!");
                return;
            }

            #endregion

            Console.WriteLine("Building ({2}) descriptor with: {0}, {1}", hidRawDevNode, hidGadgetDevNode, type);
            var descriptor = new DeviceDescriptor(hidRawDevNode, hidGadgetDevNode);
            descriptor.OpenDevNodes();
            _pipeDictionary.Add(type, descriptor);
        }

        public void StartForwarding()
        {
            if (IsForwarding) return;
            IsForwarding = true;

            _monitor.EnableReceiving();
            _tasks.Add(Task.Factory.StartNew(() =>
            {
                var pollArr = new[]
                {
                    new Pollfd
                    {
                        fd = _monitor.Fd,
                        events = PollEvents.POLLIN | PollEvents.POLLERR | PollEvents.POLLHUP
                    }
                };

                while (IsForwarding)
                {
                    var ret = Syscall.poll(pollArr, 1000);
                    if (ret == -1)
                    {
                        Console.WriteLine("Error poll monitor");
                        return;
                    }

                    if (ret == 0) continue;

                    if ((pollArr[0].revents & PollEvents.POLLIN) != 0)
                    {
                        var device = _monitor.TryReceiveDevice();
                        if (null != device)
                        {
                            var uniq = (
                                from c in device.Properties
                                where c.Key == "HID_UNIQ"
                                select c.Value
                            ).SingleOrDefault();

                            if (string.IsNullOrEmpty(uniq)) continue;
                            if (!_deviceDict.ContainsKey(uniq)) continue;

                            Console.WriteLine("{0} -> {1}", uniq, device.Action);

                            if (device.Action == "add")
                                ProcessDevice(uniq, device);
                        }
                    }
                    else if (
                        (pollArr[0].revents & PollEvents.POLLERR) > 0 ||
                        (pollArr[0].revents & PollEvents.POLLHUP) > 0
                    )
                    {
                        Console.WriteLine("monitor ERR/HUP");
                        return;
                    }

                    Thread.Yield();
                }
            }));

            foreach (var (type, descriptor) in _pipeDictionary)
                _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(descriptor, type)));
        }

        public void StopForwarding()
        {
            if (!IsForwarding) return;

            IsForwarding = false;

            Task.WaitAll(_tasks.ToArray());

            _listener.Stop();
        }

        private static unsafe long InputFilter(byte* buf, long len, Device.TypeEnum typeEnum)
        {
            switch (typeEnum)
            {
                case Device.TypeEnum.Keyboard:
                    return len;
                case Device.TypeEnum.Mouse:
                    for (var i = 1; i < len; ++i) buf[i - 1] = buf[i];
                    return len - 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeEnum), typeEnum, null);
            }
        }

        private unsafe void ForwardLoop(DeviceDescriptor descriptor, Device.TypeEnum type)
        {
            var buf = stackalloc byte[32];

            while (IsForwarding)
            {
                var fds = new HidHelperH.fd_set();
                HidHelperH.FD_ZERO(&fds);
                HidHelperH.FD_SET(descriptor.HidRawFd, &fds);
                HidHelperH.FD_SET(descriptor.HidGadgetFd, &fds);
                var fdsMax = Math.Max(descriptor.HidRawFd, descriptor.HidGadgetFd);
                var tv = new TimeH.timeval {tv_sec = 1, tv_usec = 0};

                var ret = HidHelperH.select(fdsMax + 1, ref fds, null, null, &tv);
                if (ret == -1)
                {
                    Console.WriteLine("Error select");
                    return;
                }

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

                    len = InputFilter(buf, len, type);

                    if (len > 0)
                    {
                        len = Syscall.write(descriptor.HidGadgetFd, buf, (ulong) len); // Transfer report to hidg device
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

        private class DeviceDescriptor : IDisposable
        {
            private readonly string _hidGadgetDevNode;
            private readonly string _hidRawDevNode;

            public DeviceDescriptor(string hidRawDevNode, string hidGadgetDevNode)
            {
                _hidRawDevNode = hidRawDevNode;
                _hidGadgetDevNode = hidGadgetDevNode;

                HidRawFd = -1;
                HidGadgetFd = -1;
            }

            public int HidRawFd { get; private set; }
            public int HidGadgetFd { get; private set; }

            public void Dispose()
            {
                if (HidRawFd >= 0) Syscall.close(HidRawFd);
                if (HidGadgetFd >= 0) Syscall.close(HidGadgetFd);
            }

            public void OpenDevNodes()
            {
                HidRawFd = Syscall.open(_hidRawDevNode, OpenFlags.O_RDWR);
                HidGadgetFd = Syscall.open(_hidGadgetDevNode, OpenFlags.O_RDWR);
            }
        }
    }
}