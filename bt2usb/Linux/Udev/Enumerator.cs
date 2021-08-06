using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;

using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    /// <summary>
    /// Object used to enumerate devices.
    /// </summary>
    public sealed class Enumerator : IEnumerable<Device>, IDisposable
    {
        IntPtr handle;

        /// <summary>
        /// Handle to the unmanaged instance
        /// </summary>
        public IntPtr Handle {
            get {
                if (handle == IntPtr.Zero) {
                    throw new ObjectDisposedException(null);
                }

                return handle;
            }
        }

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_enumerate_get_udev(IntPtr udev_enumerate);

        /// <summary>
        /// Gets the udev context.
        /// </summary>
        public Context Context {
            get {
                return Context.GetInstance(udev_enumerate_get_udev(Handle));
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_enumerate_get_list_entry(IntPtr udev_enumerate);

        IEnumerable<Device> List {
            get {
                var ptr = udev_enumerate_get_list_entry(Handle);
                if (ptr == IntPtr.Zero) {
                    var err = (Errno)Marshal.GetLastWin32Error();
                    if (err == Errno.ENODATA) {
                        yield break;
                    }
                    throw new UnixIOException(err);
                }

                for (var entry = ListEntry.GetInstance(ptr); entry != null; entry = entry.GetNext()) {
                    yield return new Device(Context, entry.Name);
                }
            }
        }

        /// <summary>
        /// Gets an enumerator for the matching devices
        /// </summary>
        public IEnumerator<Device> GetEnumerator() => List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_match_subsystem(IntPtr udev_enumerate, string subsystem);

        /// <summary>
        /// Match only devices belonging to a certain kernel subsystem.
        /// </summary>
        /// <param name="subsystem">
        /// filter for a subsystem of the device to include in the list
        /// </param>
        public void AddMatchSubsystem(string subsystem)
        {
            var err = udev_enumerate_add_match_subsystem(Handle, subsystem);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_nomatch_subsystem(IntPtr udev_enumerate, string subsystem);

        /// <summary>
        /// Match only devices not belonging to a certain kernel subsystem.
        /// </summary>
        /// <param name="subsystem">
        /// filter for a subsystem of the device to exclude from the list
        /// </param>
        public void AddNoMatchSubsystem(string subsystem)
        {
            var err = udev_enumerate_add_nomatch_subsystem(Handle, subsystem);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_match_sysattr(IntPtr udev_enumerate, string sysattr, string value);

        /// <summary>
        /// Match only devices with a certain /sys device attribute.
        /// </summary>
        /// <param name="sysattr">
        /// filter for a sys attribute at the device to include in the list
        /// </param>
        /// <param name="value">
        /// optional value of the sys attribute
        /// </param>
        public void AddMatchSysAttribute(string sysattr, string value = null)
        {
            var err = udev_enumerate_add_match_sysattr(Handle, sysattr, value);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_nomatch_sysattr(IntPtr udev_enumerate, string sysattr, string value);

        /// <summary>
        /// Match only devices not having a certain /sys device attribute.
        /// </summary>
        /// <param name="sysattr">
        /// filter for a sys attribute at the device to exclude from the list
        /// </param>
        /// <param name="value">
        /// optional value of the sys attribute
        /// </param>
        public void AddNoMatchSysAttribute(string sysattr, string value = null)
        {
            var err = udev_enumerate_add_nomatch_sysattr(Handle, sysattr, value);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_match_property(IntPtr udev_enumerate, string property, string value);

        /// <summary>
        /// Match only devices with a certain property.
        /// </summary>
        /// <param name="property">
        /// filter for a property of the device to include in the list
        /// </param>
        /// <param name="value">
        /// value of the property
        /// </param>
        public void AddMatchProperty(string property, string value)
        {
            var err = udev_enumerate_add_match_property(Handle, property, value);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_match_tag(IntPtr udev_enumerate, string tag);

        /// <summary>
        /// Match only devices with a certain tag.
        /// </summary>
        /// <param name="tag">
        /// filter for a tag of the device to include in the list
        /// </param>
        public void AddMatchTag(string tag)
        {
            var err = udev_enumerate_add_match_tag(Handle, tag);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName)]
        static extern int udev_enumerate_add_match_parent(IntPtr udev_enumerate, IntPtr parent);

        /// <summary>
        /// Return the devices on the subtree of one given device. The parent
        /// itself is included in the list.
        /// </summary>
        /// <param name="parent">
        /// parent device where to start searching
        /// </param>
        public void AddMatchParent(Device parent)
        {
            var err = udev_enumerate_add_match_parent(Handle, parent?.Handle ?? IntPtr.Zero);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName)]
        static extern int udev_enumerate_add_match_is_initialized(IntPtr udev_enumerate);

        /// <summary>
        /// Match only devices which udev has set up already. This makes
        /// sure, that the device node permissions and context are properly set
        /// and that network devices are fully renamed.
        /// </summary>
        /// <remarks>
        /// Usually, devices which are found in the kernel but not already
        /// handled by udev, have still pending events. Services should subscribe
        /// to monitor events and wait for these devices to become ready, instead
        /// of using uninitialized devices.
        ///
        /// For now, this will not affect devices which do not have a device node
        /// and are not network interfaces.
        /// </remarks>
        public void AddMatchIsInitialized()
        {
            var err = udev_enumerate_add_match_is_initialized(Handle);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_match_sysname(IntPtr udev_enumerate, string sysname);

        /// <summary>
        /// Match only devices with a given /sys device name.
        /// </summary>
        /// <param name="sysname">
        /// filter for the name of the device to include in the list
        /// </param>
        public void AddMatchSysName(string sysname)
        {
            var err = udev_enumerate_add_match_sysname(Handle, sysname);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_enumerate_add_syspath(IntPtr udev_enumerate, string syspath);

        /// <summary>
        /// Add a device to the list of devices, to retrieve it back sorted in dependency order.
        /// </summary>
        /// <param name="syspath">
        /// path of a device
        /// </param>
        public void AddSysPath(string syspath)
        {
            var err = udev_enumerate_add_syspath(Handle, syspath);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName)]
        static extern int udev_enumerate_scan_devices(IntPtr udev_enumerate);

        /// <summary>
        /// Scan /sys for all devices which match the given filters. No matches
        /// will return all currently available devices.
        /// </summary>
        public void ScanDevices()
        {
            var err = udev_enumerate_scan_devices(Handle);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName)]
        static extern int udev_enumerate_scan_subsystems(IntPtr udev_enumerate);

        /// <summary>
        /// Scan /sys for all kernel subsystems, including buses, classes, drivers.
        /// </summary>
        public void ScanSubsystems()
        {
            var err = udev_enumerate_scan_subsystems(Handle);
            if (err < 0) {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_enumerate_new(IntPtr udev);

        static IntPtr New(Context udev)
        {
            if (udev == null) {
                throw new ArgumentNullException(nameof(udev));
            }
            var handle = udev_enumerate_new(udev.Handle);
            if (handle == null) {
                throw new UnixIOException(Marshal.GetLastWin32Error());
            }

            return handle;
        }

        /// <summary>
        /// Create a new instance of <see cref="Enumerator" /> using the specified <see cref="Context" />
        /// </summary>
        public Enumerator(Context udev) : this(New(udev))
        {
        }

        Enumerator(IntPtr handle)
        {
            this.handle = handle;
        }

        #region IDisposable Support

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_enumerate_unref(IntPtr udev_enumerate);

        void Dispose(bool disposing)
        {
            if (handle != IntPtr.Zero) {
               udev_enumerate_unref(handle);
               handle = IntPtr.Zero;
            }
        }

        /// <inheritdoc />
        ~Enumerator()
        {
            Dispose(false);
        }

       /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
