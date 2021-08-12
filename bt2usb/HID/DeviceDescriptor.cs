using System;
using static Mono.Unix.Native.OpenFlags;
using static Mono.Unix.Native.Syscall;

namespace bt2usb.HID
{
    public class DeviceDescriptor : IDisposable
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
            if (HidRawFd >= 0) close(HidRawFd);
            if (HidGadgetFd >= 0) close(HidGadgetFd);
        }

        public void OpenDevNodes()
        {
            HidRawFd = open(_hidRawDevNode, O_RDWR | O_EXCL);
            HidGadgetFd = open(_hidGadgetDevNode, O_RDWR | O_EXCL);
        }
    }
}