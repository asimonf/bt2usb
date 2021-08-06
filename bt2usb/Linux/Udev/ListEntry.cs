using System;
using System.Runtime.InteropServices;
using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    sealed class ListEntry
    {
        IntPtr handle;

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_list_entry_get_next(IntPtr udev_list_entry);

        public ListEntry GetNext()
        {
            var next = udev_list_entry_get_next(handle);
            return GetInstance(next);
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        static extern IntPtr udev_list_entry_get_by_name(IntPtr udev_list_entry, string name);

        public ListEntry GetByName(string name)
        {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            var match = udev_list_entry_get_by_name(handle, name);
            return GetInstance(match);
        }

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_list_entry_get_name(IntPtr udev_list_entry);

        public string Name {
            get {
                var name = udev_list_entry_get_name(handle);
                if (name == IntPtr.Zero) {
                    return null;
                }
                return Marshal.PtrToStringAnsi(name);
            }
        }

        [DllImport(UdevLibraryName)]
        static extern IntPtr udev_list_entry_get_value(IntPtr udev_list_entry);
        
        public string Value {
            get {
                var value = udev_list_entry_get_value(handle);
                if (value == IntPtr.Zero) {
                    return null;
                }
                return Marshal.PtrToStringAnsi(value);
            }
        }

        ListEntry(IntPtr handle)
        {
            this.handle = handle;
        }

        internal static ListEntry GetInstance(IntPtr entry)
        {
            if (entry == IntPtr.Zero) {
                return null;
            }
            return new ListEntry(entry);
        }
    }
}
