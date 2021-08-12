using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.Linux;
using bt2usb.Server.Model;
using static bt2usb.Linux.HID.HidHelperH;
using static Mono.Unix.Native.Syscall;

namespace bt2usb.HID
{
    public class HidForwarder : IDisposable
    {
        private class ForwardingTask : IDisposable
        {
            public Task Task;
            public CancellationTokenSource CancellationTokenSource;

            public void Dispose()
            {
                CancellationTokenSource?.Cancel();
                Task?.Wait();
                Task?.Dispose();
                CancellationTokenSource?.Dispose();
            }
        }

        private readonly ConcurrentDictionary<string, ForwardingTask> _tasks =
            new ConcurrentDictionary<string, ForwardingTask>();

        public void AddDescriptor(string uniq, DeviceDescriptor descriptor, BtDeviceType type)
        {
            if (_tasks.TryRemove(uniq, out var task))
            {
                task.Dispose();
            }

            var tokenSource = new CancellationTokenSource();

            _tasks.TryAdd(uniq, new ForwardingTask
            {
                Task = Task.Factory.StartNew(() => ForwardLoop(tokenSource.Token, descriptor, type)),
                CancellationTokenSource = tokenSource,
            });
        }

        public void DeleteDescriptor(string uniq)
        {
            if (_tasks.TryRemove(uniq, out var task))
            {
                task.Dispose();                
            }
        }

        private static unsafe long InputFilter(byte* buf, long len, BtDeviceType typeEnum)
        {
            switch (typeEnum)
            {
                case BtDeviceType.Keyboard:
                    return len;
                case BtDeviceType.Mouse:
                    for (var i = 1; i < len; i++)
                        buf[i - 1] = buf[i];

                    return len - 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeEnum), typeEnum, null);
            }
        }

        private static unsafe void ForwardLoop(CancellationToken cancellationToken,
            DeviceDescriptor descriptor,
            BtDeviceType type)
        {
            const int bufSize = 96;
            var buf = stackalloc byte[bufSize];

            while (!cancellationToken.IsCancellationRequested)
            {
                var fds = new fd_set();
                FD_ZERO(&fds);
                FD_SET(descriptor.HidRawFd, &fds);
                FD_SET(descriptor.HidGadgetFd, &fds);
                var fdsMax = Math.Max(descriptor.HidRawFd, descriptor.HidGadgetFd);
                var tv = new TimeH.timeval {tv_sec = 1, tv_usec = 0};

                var ret = select(fdsMax + 1, ref fds, null, null, &tv);
                if (ret == -1)
                {
                    Console.WriteLine("Error select");
                    return;
                }

                // Direction: hidRawFd -> hidgFd
                if (FD_ISSET(descriptor.HidRawFd, &fds))
                {
                    var len = read(descriptor.HidRawFd, buf, bufSize);
                    if (len < 0)
                    {
                        Console.WriteLine("Error read");
                        return;
                    }

                    len = InputFilter(buf, len, type);

                    if (len > 0)
                    {
                        len = write(descriptor.HidGadgetFd, buf, (ulong) len); // Transfer report to hidg device
                        if (len < 0)
                        {
                            Console.WriteLine("Error write");
                            return;
                        }
                    }
                }

                // Direction: hidgFd -> hidRawFd
                if (FD_ISSET(descriptor.HidGadgetFd, &fds))
                {
                    var len = read(descriptor.HidGadgetFd, buf, bufSize);
                    if (len < 0)
                    {
                        Console.WriteLine("Error read");
                        return;
                    }

                    var bufSpan = new ReadOnlySpan<byte>(buf, (int) len);
                    Console.WriteLine("[out] -> {0}", BitConverter.ToString(bufSpan.ToArray()));
                }

                Task.Yield();
            }
        }

        public void Dispose()
        {
            foreach (var (_, task) in _tasks)
            {
                task.Dispose();
            }

            _tasks.Clear();
        }
    }
}