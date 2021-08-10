using System;
using System.Runtime.InteropServices;
using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    internal sealed class ListEntry
    {
        private readonly IntPtr handle;

        private ListEntry(IntPtr handle)
        {
            this.handle = handle;
        }

        public string Name
        {
            get
            {
                var name = udev_list_entry_get_name(handle);
                if (name == IntPtr.Zero) return null;
                return Marshal.PtrToStringAnsi(name);
            }
        }

        public string Value
        {
            get
            {
                var value = udev_list_entry_get_value(handle);
                if (value == IntPtr.Zero) return null;
                return Marshal.PtrToStringAnsi(value);
            }
        }

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_list_entry_get_next(IntPtr udev_list_entry);

        public ListEntry GetNext()
        {
            var next = udev_list_entry_get_next(handle);
            return GetInstance(next);
        }

        [DllImport(UdevLibraryName, CharSet = CharSet.Ansi)]
        private static extern IntPtr udev_list_entry_get_by_name(IntPtr udev_list_entry, string name);

        public ListEntry GetByName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            var match = udev_list_entry_get_by_name(handle, name);
            return GetInstance(match);
        }

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_list_entry_get_name(IntPtr udev_list_entry);

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_list_entry_get_value(IntPtr udev_list_entry);

        internal static ListEntry GetInstance(IntPtr entry)
        {
            if (entry == IntPtr.Zero) return null;
            return new ListEntry(entry);
        }
    }
}