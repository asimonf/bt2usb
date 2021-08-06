// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global

using System.Runtime.InteropServices;

namespace bt2usb.Linux
{
    public static class IoctlH
    {
        public const uint _IOC_NRBITS = 8;
        public const uint _IOC_TYPEBITS = 8;
        public const uint _IOC_SIZEBITS = 13;
        public const uint _IOC_DIRBITS = 3;

        public const uint _IOC_NRMASK = (1 << (int) _IOC_NRBITS) - 1;
        public const uint _IOC_TYPEMASK = (1 << (int) _IOC_TYPEBITS) - 1;
        public const uint _IOC_SIZEMASK = (1 << (int) _IOC_SIZEBITS) - 1;
        public const uint _IOC_DIRMASK = (1 << (int) _IOC_DIRBITS) - 1;

        public const uint _IOC_NRSHIFT = 0;
        public const uint _IOC_TYPESHIFT = _IOC_NRSHIFT + _IOC_NRBITS;
        public const uint _IOC_SIZESHIFT = _IOC_TYPESHIFT + _IOC_TYPEBITS;
        public const uint _IOC_DIRSHIFT = _IOC_SIZESHIFT + _IOC_SIZEBITS;

        public const uint _IOC_NONE = 0;
        public const uint _IOC_READ = 1;
        public const uint _IOC_WRITE = 2;

        public static uint _IOC(uint dir, uint type, uint nr, uint size)
        {
            return
                (dir << (int) _IOC_DIRSHIFT) |
                (type << (int) _IOC_TYPESHIFT) |
                (nr << (int) _IOC_NRSHIFT) |
                (size << (int) _IOC_SIZESHIFT);
        }

        public static uint _IOC_TYPECHECK<T>()
        {
            return (uint) Marshal.SizeOf(typeof(T));
        }

        public static uint _IO(uint type, uint nr)
        {
            return _IOC(_IOC_NONE, type, nr, 0);
        }

        public static uint _IOR<T>(uint type, uint nr)
        {
            return _IOC(_IOC_READ, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOW<T>(uint type, uint nr)
        {
            return _IOC(_IOC_WRITE, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOWR<T>(uint type, uint nr)
        {
            return _IOC(_IOC_READ | _IOC_WRITE, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOR_BAD<T>(uint type, uint nr) where T : unmanaged
        {
            return _IOC(_IOC_READ, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOW_BAD<T>(uint type, uint nr) where T : unmanaged
        {
            return _IOC(_IOC_WRITE, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOWR_BAD<T>(uint type, uint nr) where T : unmanaged
        {
            return _IOC(_IOC_READ | _IOC_WRITE, type, nr, _IOC_TYPECHECK<T>());
        }

        public static uint _IOC_DIR(uint nr)
        {
            return (nr >> (int) _IOC_DIRSHIFT) & _IOC_DIRMASK;
        }

        public static uint _IOC_TYPE(uint nr)
        {
            return (nr >> (int) _IOC_TYPESHIFT) & _IOC_TYPEMASK;
        }

        public static uint _IOC_NR(uint nr)
        {
            return (nr >> (int) _IOC_NRSHIFT) & _IOC_NRMASK;
        }

        public static uint _IOC_SIZE(uint nr)
        {
            return (nr >> (int) _IOC_SIZESHIFT) & _IOC_SIZEMASK;
        }

        public static uint IOC_IN()
        {
            return _IOC_WRITE << (int) _IOC_DIRSHIFT;
        }

        public static uint IOC_OUT()
        {
            return _IOC_READ << (int) _IOC_DIRSHIFT;
        }

        public static uint IOC_INOUT()
        {
            return (_IOC_WRITE | _IOC_READ) << (int) _IOC_DIRSHIFT;
        }

        public static uint IOCSIZE_MASK()
        {
            return _IOC_SIZEMASK << (int) _IOC_SIZESHIFT;
        }

        public static uint IOCSIZE_SHIFT()
        {
            return _IOC_SIZESHIFT;
        }
    }
}