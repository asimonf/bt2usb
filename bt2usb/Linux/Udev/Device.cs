using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Unix;
using Mono.Unix.Native;
using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    /// <summary>
    /// Representation of kernel sys devices.
    /// </summary>
    /// <remarks>
    /// Devices are uniquely identified
    /// by their syspath, every device has exactly one path in the kernel sys
    /// filesystem. Devices usually belong to a kernel subsystem, and have
    /// a unique name inside that subsystem.
    /// </remarks>
    public sealed class Device : IDisposable
    {
        private IntPtr _handle;

        internal IntPtr Handle
        {
            get
            {
                if (_handle == IntPtr.Zero)
                {
                    throw new ObjectDisposedException(null);
                }

                return _handle;
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern ulong udev_device_get_seqnum(IntPtr udev_device);

        /// <summary>
        /// Gets the kernel event sequence number, or 0 if there is no sequence number available.
        /// </summary>
        /// <remarks>
        /// This is only valid if the device was received through a monitor. Devices read from
        /// sys do not have a sequence number.
        /// </remarks>
        public ulong SequenceNumber
        {
            get
            {
                var seqnum = udev_device_get_seqnum(Handle);
                var err = Marshal.GetLastWin32Error();
                if (err > 0)
                {
                    throw new UnixIOException(err);
                }

                return seqnum;
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_driver(IntPtr udev_device);

        /// <summary>
        /// Gets the driver name or <c>null</c> if there is no driver attached.
        /// </summary>
        public string Driver
        {
            get
            {
                var driver = udev_device_get_driver(Handle);
                if (driver == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(driver);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_devtype(IntPtr udev_device);

        /// <summary>
        /// Gets the devtype of the udev device.
        /// </summary>
        public string DevType
        {
            get
            {
                var devtype = udev_device_get_devtype(Handle);
                if (devtype == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(devtype);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_subsystem(IntPtr udev_device);

        /// <summary>
        /// Gets the subsystem string of the udev device.
        /// </summary>
        /// <remarks>
        /// The string does not contain any "/".
        /// </remarks>
        public string Subsystem
        {
            get
            {
                var subsystem = udev_device_get_subsystem(Handle);
                if (subsystem == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(subsystem);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr udev_device_get_property_value(IntPtr udev_device, string key);

        /// <summary>
        /// Gets the value of a given property.
        /// </summary>
        /// <param name="key">The property name</param>
        public string this[string key]
        {
            get
            {
                var value = udev_device_get_property_value(Handle, key);
                if (value == IntPtr.Zero)
                {
                    var err = Marshal.GetLastWin32Error();
                    if (err > 0)
                    {
                        throw new UnixIOException(Marshal.GetLastWin32Error());
                    }

                    throw new ArgumentException("Property not found", nameof(key));
                }

                return Marshal.PtrToStringAnsi(value);
            }
        }

        /// <summary>
        /// Gets the parent device or <c>null</c>, if it no parent exists.
        /// </summary>
        /// <remarks>
        /// It is not necessarily just the upper level directory, empty or not
        /// recognized sys directories are ignored.
        /// </remarks>
        public Device Parent => lazyParent.Value;

        readonly Lazy<Device> lazyParent;

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_parent(IntPtr udev_device);

        Device getParent()
        {
            var parent = udev_device_get_parent(Handle);
            if (parent == IntPtr.Zero)
            {
                throw new UnixIOException(Marshal.GetLastWin32Error());
            }

            return new Device(udev_device_ref(parent));
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr udev_device_get_parent_with_subsystem_devtype(IntPtr udev_device, string subsystem,
            string devtype);

        /// <summary>
        /// Find the next parent device, with a matching subsystem and devtype
        /// value.
        /// </summary>
        /// <param name="subsystem">The subsystem of the device.</param>
        /// <param name="devtype">The type (DEVTYPE) of the device.</param>
        /// <returns>A new udev device</returns>
        /// <remarks>
        /// If devtype is <c>null</c>, only subsystem is checked, and any
        /// devtype will match.
        /// </remarks>
        public Device TryGetAncestor(string subsystem, string devtype = null)
        {
            if (subsystem == null)
            {
                throw new ArgumentNullException(nameof(subsystem));
            }

            var parent = udev_device_get_parent_with_subsystem_devtype(Handle, subsystem, devtype);
            if (parent == IntPtr.Zero)
            {
                var err = (Errno) Marshal.GetLastWin32Error();
                if (err == Errno.ENOENT)
                {
                    return null;
                }

                throw new UnixIOException(err);
            }

            return new Device(udev_device_ref(parent));
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_udev(IntPtr udev_device);

        /// <summary>
        /// the udev library context the device was created with.
        /// </summary>
        internal Context Context
        {
            get
            {
                var udev = udev_device_get_udev(Handle);
                if (udev == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Context.GetInstance(udev);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_devpath(IntPtr udev_device);

        /// <summary>
        /// Retrieve the kernel devpath value of the udev device.
        /// </summary>
        /// <remarks>
        /// The path does not contain the sys mount point, and starts with a '/'.
        /// </remarks>
        public string DevPath
        {
            get
            {
                var devpath = udev_device_get_devpath(Handle);
                if (devpath == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(devpath);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_syspath(IntPtr udev_device);

        /// <summary>
        /// Gets the sys path of the udev device.
        /// </summary>
        /// <remarks>
        /// The path is an absolute path and starts with the sys mount point.
        /// </remarks>
        public string SysPath
        {
            get
            {
                var syspath = udev_device_get_syspath(Handle);
                if (syspath == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(syspath);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_sysname(IntPtr udev_device);

        /// <summary>
        /// Gets the kernel device name in /sys.
        /// </summary>
        public string SysName
        {
            get
            {
                var sysname = udev_device_get_sysname(Handle);
                if (sysname == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(sysname);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_sysnum(IntPtr udev_device);

        /// <summary>
        /// Gets the instance number of the device.
        /// </summary>
        /// <remarks>
        /// This is the trailing number string of the device name
        /// </remarks>
        public string SysNum
        {
            get
            {
                var sysnum = udev_device_get_sysnum(Handle);
                if (sysnum == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                return Marshal.PtrToStringAnsi(sysnum);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_devnode(IntPtr udev_device);

        /// <summary>
        /// Gets the device node file name belonging to the udev device
        /// or <c>null</c> if no device node exists.
        /// </summary>
        /// <remarks>
        /// The path is an absolute path, and starts with the device directory.
        /// </remarks>
        public string DevNode
        {
            get
            {
                var devnode = udev_device_get_devnode(Handle);
                if (devnode == IntPtr.Zero)
                {
                    var err = (Errno) Marshal.GetLastWin32Error();
                    if (err != Errno.ENOENT)
                    {
                        throw new UnixIOException(err);
                    }

                    return null;
                }

                return Marshal.PtrToStringAnsi(devnode);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_devlinks_list_entry(IntPtr udev_device);

        /// <summary>
        /// Gets the list of device links pointing to the device file of
        /// the udev device.
        /// </summary>
        /// <remarks>
        /// The path is an absolute path, and starts with the device directory.
        /// </remarks>
        public IEnumerable<string> DevLinks
        {
            get
            {
                var ptr = udev_device_get_devlinks_list_entry(Handle);
                if (ptr == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                for (var entry = ListEntry.GetInstance(ptr); entry != null; entry = entry.GetNext())
                {
                    yield return entry.Name;
                }
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_properties_list_entry(IntPtr udev_device);

        /// <summary>
        /// Gets the list of key/value device properties of the udev device.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Properties
        {
            get
            {
                var ptr = udev_device_get_properties_list_entry(Handle);
                if (ptr == null)
                {
                    var err = (Errno) Marshal.GetLastWin32Error();
                    if (err != Errno.ENOENT)
                    {
                        yield break;
                    }

                    throw new UnixIOException(err);
                }

                for (var entry = ListEntry.GetInstance(ptr); entry != null; entry = entry.GetNext())
                {
                    yield return new KeyValuePair<string, string>(entry.Name, entry.Value);
                }
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_action(IntPtr udev_device);

        /// <summary>
        /// Gets the kernel action value, or <c>null</c> if there is no action value available.
        /// </summary>
        /// <remarks>
        /// This is only valid if the device was received through a monitor. Devices read from
        /// sys do not have an action string. Usual actions are: add, remove, change, online,
        /// offline.
        /// </remarks>
        public string Action
        {
            get
            {
                var action = udev_device_get_action(Handle);
                if (action == IntPtr.Zero)
                {
                    var err = (Errno) Marshal.GetLastWin32Error();
                    if (err == Errno.ENOENT)
                    {
                        return null;
                    }

                    throw new UnixIOException(err);
                }

                return Marshal.PtrToStringAnsi(action);
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr udev_device_get_sysattr_value(IntPtr udev_device, [MarshalAs(UnmanagedType.LPStr)] string sysattr);

        /// <summary>
        /// Gets the content of a sys attribute file, or <c>null</c> if there is no sys attribute value.
        /// </summary>
        /// <param name="name">attribute name</param>
        /// <remarks>
        /// The retrieved value is cached in the device. Repeated calls will return the same
        /// value and not open the attribute again.
        /// </remarks>
        public byte[] TryGetAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            var value = udev_device_get_sysattr_value(Handle, name);
            if (value == IntPtr.Zero)
            {
                var err = (Errno) Marshal.GetLastWin32Error();
                if (err == Errno.ENOENT)
                {
                    return null;
                }

                throw new UnixIOException(err);
            }

            unsafe
            {
                var ptr = (byte*)value.ToPointer();
                var size = 0;
                
                while (ptr[size] != 0)
                {
                    size++;
                }

                var ret = new byte[size];
                Marshal.Copy(value, ret, 0, size);

                return ret;
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern int udev_device_set_sysattr_value(IntPtr udev_device, string sysattr, string value);

        /// <summary>
        /// Update the contents of the sys attribute and the cached value of the device.
        /// </summary>
        /// <param name="name">attribute name</param>
        /// <param name="value">new value to be set</param>
        public void SetAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var err = udev_device_set_sysattr_value(Handle, name, value);
            if (err < 0)
            {
                throw new UnixIOException(-err);
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_sysattr_list_entry(IntPtr udev_device);

        /// <summary>
        /// Gets the list of available sysfs attributes.
        /// </summary>
        public IEnumerable<string> AttributeNames
        {
            get
            {
                var ptr = udev_device_get_sysattr_list_entry(Handle);
                if (ptr == IntPtr.Zero)
                {
                    throw new UnixIOException(Marshal.GetLastWin32Error());
                }

                for (var entry = ListEntry.GetInstance(ptr); entry != null; entry = entry.GetNext())
                {
                    yield return entry.Name;
                }
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern bool udev_device_get_is_initialized(IntPtr udev_device);

        /// <summary>
        /// Checks if udev has already handled the device and has set up
        /// device node permissions and context, or has renamed a network
        /// device.
        /// </summary>
        /// <remarks>
        /// This is only implemented for devices with a device node
        /// or network interfaces. All other devices return <c>true</c> here.
        /// </remarks>
        public bool IsInitialized
        {
            get { return udev_device_get_is_initialized(Handle); }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        static extern IntPtr udev_device_get_tags_list_entry(IntPtr udev_device);

        /// <summary>
        /// Gets  the list of tags attached to the udev device.
        /// </summary>
        public IEnumerable<string> Tags
        {
            get
            {
                var ptr = udev_device_get_tags_list_entry(Handle);
                if (ptr == IntPtr.Zero)
                {
                    var err = (Errno) Marshal.GetLastWin32Error();
                    if (err == Errno.ENOENT)
                    {
                        yield break;
                    }

                    throw new UnixIOException((int) err);
                }

                for (var entry = ListEntry.GetInstance(ptr); entry != null; entry = entry.GetNext())
                {
                    yield return entry.Name;
                }
            }
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern bool udev_device_has_tag(IntPtr udev_device, string tag);

        /// <summary>
        /// Check if a given device has a certain tag associated.
        /// </summary>
        /// <param name="tag">The tag name</param>
        /// <returns>
        /// <c>true</c> if the <paramref name="tag" /> is found. <c>false</c> otherwise.
        /// </returns>
        public bool HasTag(string tag)
        {
            return udev_device_has_tag(Handle, tag);
        }

        internal Device(IntPtr device)
        {
            this._handle = device;
            lazyParent = new Lazy<Device>(getParent);
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr udev_device_new_from_syspath(IntPtr udev_device, string syspath);

        static IntPtr New(Context udev, string syspath)
        {
            if (udev == null)
            {
                throw new ArgumentNullException(nameof(udev));
            }

            if (syspath == null)
            {
                throw new ArgumentNullException(nameof(syspath));
            }

            var device = udev_device_new_from_syspath(udev.Handle, syspath);
            if (device == IntPtr.Zero)
            {
                throw new UnixIOException(Marshal.GetLastWin32Error());
            }

            return device;
        }

        /// <summary>
        /// Create new udev device, and fill in information from the sys
        /// device and the udev database entry. The syspath is the absolute
        /// path to the device, including the sys mount point.
        /// </summary>
        public Device(Context udev, string syspath) : this(New(udev, syspath))
        {
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi, SetLastError = true)]
        static extern IntPtr udev_device_new_from_subsystem_sysname(IntPtr udev_device, string subsystem,
            string sysname);

        static IntPtr New(Context udev, string subsystem, string sysname)
        {
            if (udev == null)
            {
                throw new ArgumentNullException(nameof(udev));
            }

            if (subsystem == null)
            {
                throw new ArgumentNullException(nameof(subsystem));
            }

            if (sysname == null)
            {
                throw new ArgumentNullException(nameof(sysname));
            }

            var device = udev_device_new_from_subsystem_sysname(udev.Handle, subsystem, sysname);
            if (device == IntPtr.Zero)
            {
                throw new UnixIOException(Marshal.GetLastWin32Error());
            }

            return device;
        }

        /// <summary>
        /// Create new udev device, and fill in information from the sys device
        /// and the udev database entry. The device is looked up by the subsystem
        /// and name string of the device, like "mem" / "zero", or "block" / "sda".
        /// </summary>
        public Device(Context udev, string subsystem, string sysname) : this(New(udev, subsystem, sysname))
        {
        }

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_device_ref(IntPtr udev_device);

        internal static Device GetInstance(IntPtr device)
        {
            return new Device(udev_device_ref(device));
        }

        #region IDisposable Support

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_device_unref(IntPtr udev_device);

        void Dispose(bool disposing)
        {
            if (_handle != IntPtr.Zero)
            {
                udev_device_unref(_handle);
                _handle = IntPtr.Zero;
            }
        }

        /// <inheritdoc />
        ~Device()
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

        /// <inheritdoc />
        public override string ToString()
        {
            return DevPath;
        }
    }
}