using System;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;
using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    /// <summary>
    ///     Udev device event source
    /// </summary>
    public sealed class Monitor : IDisposable
    {
        private IntPtr handle;

        private Monitor(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        ///     Create new udev monitor and connect to a specified event source.
        /// </summary>
        /// <param name="udev">udev library context</param>
        /// <param name="sourceName">the name of the event source</param>
        /// <remarks>
        ///     Applications should usually not connect directly to the
        ///     <see cref="EventSource.Kernel" /> events, because the devices
        ///     might not be useable at that time, before udev has configured
        ///     them, and created device nodes. Accessing devices at the same
        ///     time as udev, might result in unpredictable behavior. The
        ///     <see cref="EventSource.Udev" /> events are sent out after udev
        ///     has finished its event processing, all rules have been processed,
        ///     and needed device nodes are created.
        /// </remarks>
        public Monitor(Context udev, string sourceName = EventSource.Udev)
            : this(New(udev, sourceName))
        {
        }

        /// <summary>
        ///     Pointer to the unmanaged udev monitor instance
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                if (handle == IntPtr.Zero) throw new ObjectDisposedException(null);
                return handle;
            }
        }

        /// <summary>
        ///     Gets or sets the file descriptor is blocking.
        /// </summary>
        public bool Blocking
        {
            get
            {
                var flags = getFlags();
                return !flags.HasFlag(OpenFlags.O_NONBLOCK);
            }
            set
            {
                var flags = getFlags();

                if (value)
                    flags &= ~OpenFlags.O_NONBLOCK;
                else
                    flags |= OpenFlags.O_NONBLOCK;

                setFlags(flags);
            }
        }

        /// <summary>
        ///     Gets the udev context.
        /// </summary>
        public Context Context
        {
            get
            {
                var udev = udev_monitor_get_udev(Handle);
                return Context.GetInstance(udev);
            }
        }

        /// <summary>
        ///     Gets the socket file descriptor associated with the monitor.
        /// </summary>
        public int Fd
        {
            get
            {
                var fd = udev_monitor_get_fd(Handle);
                if (fd < 0) throw new UnixIOException(-fd);
                return fd;
            }
        }

        private OpenFlags getFlags()
        {
            var ret = Syscall.fcntl(Fd, FcntlCommand.F_GETFL);
            if (ret == -1) throw new UnixIOException(Marshal.GetLastWin32Error());
            return (OpenFlags) ret;
        }

        private void setFlags(OpenFlags flags)
        {
            var ret = Syscall.fcntl(Fd, FcntlCommand.F_SETFL, (int) flags);
            if (ret == -1) throw new UnixIOException(Marshal.GetLastWin32Error());
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr udev_monitor_new_from_netlink(IntPtr udev, string name);

        private static IntPtr New(Context udev, string name)
        {
            if (udev == null) throw new ArgumentNullException(nameof(udev));
            if (name == null) throw new ArgumentNullException(nameof(name));
            var monitor = udev_monitor_new_from_netlink(udev.Handle, name);
            if (monitor == IntPtr.Zero) throw new UnixIOException(Marshal.GetLastWin32Error());
            return monitor;
        }

        [DllImport(UdevLibraryName)]
        private static extern int udev_monitor_filter_update(IntPtr udev_monitor);

        /// <summary>
        ///     Update the installed socket filter. This is only needed,
        ///     if the filter was removed or changed.
        /// </summary>
        public void UpdateFilter()
        {
            var err = udev_monitor_filter_update(Handle);
            if (err < 0) throw new UnixIOException(-err);
        }

        [DllImport(UdevLibraryName)]
        private static extern int udev_monitor_enable_receiving(IntPtr udev_monitor);

        /// <summary>
        ///     Binds the udev monitor socket to the event source.
        /// </summary>
        public void EnableReceiving()
        {
            var err = udev_monitor_enable_receiving(Handle);
            if (err < 0) throw new UnixIOException(-err);
        }

        [DllImport(UdevLibraryName)]
        private static extern int udev_monitor_set_receive_buffer_size(IntPtr udev_monitor, int size);

        /// <summary>
        ///     Set the size of the kernel socket buffer. This call needs the
        ///     appropriate privileges to succeed.
        /// </summary>
        public void SetReceiveBufferSize(int size)
        {
            var err = udev_monitor_set_receive_buffer_size(Handle, size);
            if (err < 0) throw new UnixIOException(-err);
        }

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_monitor_get_udev(IntPtr udev_monitor);

        [DllImport(UdevLibraryName)]
        private static extern int udev_monitor_get_fd(IntPtr udev_monitor);

        [DllImport(UdevLibraryName, SetLastError = true)]
        private static extern IntPtr udev_monitor_receive_device(IntPtr udev_monitor);

        /// <summary>
        ///     Receive data from the udev monitor socket, allocate a new udev
        ///     device, fill in the received data, and return the device.
        /// </summary>
        /// <returns>
        ///     a new udev device, or <c>null</c>
        /// </returns>
        /// <remarks>
        ///     The monitor socket is by default set to NONBLOCK. A variant of poll() on
        ///     the file descriptor returned by <see cref="P:Fd" /> should to be used to
        ///     wake up when new devices arrive, or alternatively the file descriptor
        ///     switched into blocking mode.
        /// </remarks>
        public Device TryReceiveDevice()
        {
            var device = udev_monitor_receive_device(Handle);
            if (device == IntPtr.Zero)
            {
                var err = (Errno) Marshal.GetLastWin32Error();
                if (err == Errno.EWOULDBLOCK) return null;
                throw new UnixIOException(err);
            }

            return new Device(device);
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        private static extern int udev_monitor_filter_add_match_subsystem_devtype(IntPtr udev_monitor, string subsystem,
            string devtype);

        /// <summary>
        ///     This filter is efficiently executed inside the kernel, and libudev subscribers
        ///     will usually not be woken up for devices which do not match.
        /// </summary>
        /// <param name="subsystem">the subsystem value to match the incoming devices against</param>
        /// <param name="devtype">the devtype value to match the incoming devices against</param>
        /// <remarks>
        ///     The filter must be installed before the monitor is switched to listening mode.
        /// </remarks>
        public void AddMatchSubsystem(string subsystem, string devtype = null)
        {
            if (subsystem == null) throw new ArgumentNullException(nameof(subsystem));
            var err = udev_monitor_filter_add_match_subsystem_devtype(Handle, subsystem, devtype);
            if (err < 0) throw new UnixIOException(-err);
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        private static extern int udev_monitor_filter_add_match_tag(IntPtr udev_monitor, string tag);

        /// <summary>
        ///     This filter is efficiently executed inside the kernel, and libudev subscribers
        ///     will usually not be woken up for devices which do not match.
        /// </summary>
        /// <param name="tag">the name of a tag</param>
        /// <remarks>
        ///     The filter must be installed before the monitor is switched to listening mode.
        /// </remarks>
        public void AddMatchTag(string tag)
        {
            var err = udev_monitor_filter_add_match_tag(Handle, tag);
            if (err < 0) throw new UnixIOException(-err);
        }

        [DllImport(UdevLibraryName)]
        private static extern int udev_monitor_filter_remove(IntPtr udev_monitor);

        /// <summary>
        ///     Remove all filters from monitor.
        /// </summary>
        public void RemoveFilters()
        {
            var err = udev_monitor_filter_remove(Handle);
            if (err < 0) throw new UnixIOException(-err);
        }

        /// <summary>
        ///     Contains valid strings to pass to <see cref="Monitor" />
        /// </summary>
        public static class EventSource
        {
            /// <summary>
            ///     Events are sent out after udev has finished its event processing,
            ///     all rules have been processed, and needed device nodes are created.
            /// </summary>
            public const string Udev = "udev";

            /// <summary>
            ///     Events are sent out before udev has finished its event processing.
            /// </summary>
            public const string Kernel = "kernel";
        }

        #region IDisposable Support

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_monitor_unref(IntPtr udev_monitor);

        private void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero)
            {
                udev_monitor_unref(handle);
                handle = IntPtr.Zero;
            }
        }

        /// <inherit />
        ~Monitor()
        {
            Dispose(false);
        }

        /// <inherit />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}