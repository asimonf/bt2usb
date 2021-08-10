using System.Runtime.InteropServices;

namespace bt2usb.Linux.HID
{
    public static unsafe class HidHelperH
    {
        public const int FD_SETSIZE = 1024;
        public const int NFDBITS = 8 * sizeof(int);

        private static int __FD_ELT(int d)
        {
            return d / NFDBITS;
        }

        private static int __FD_MASK(int d)
        {
            return (int) (1U << (d % NFDBITS));
        }

        private static int* __FDS_BITS(fd_set* set)
        {
            return set->__fds_bits;
        }

        public static void FD_SET(int d, fd_set* set)
        {
            __FDS_BITS(set)[__FD_ELT(d)] |= __FD_MASK(d);
        }

        public static void FD_CLR(int d, fd_set* set)
        {
            __FDS_BITS(set)[__FD_ELT(d)] &= ~__FD_MASK(d);
        }

        public static bool FD_ISSET(int d, fd_set* set)
        {
            return (__FDS_BITS(set)[__FD_ELT(d)] & __FD_MASK(d)) != 0;
        }

        public static void FD_ZERO(fd_set* set)
        {
            for (uint i = 0; i < sizeof(fd_set) / sizeof(int); ++i) __FDS_BITS(set)[i] = 0;
        }

        [DllImport("c", SetLastError = true)]
        public static extern int select(
            int __nfds,
            ref fd_set __readfds,
            fd_set* __writefds,
            fd_set* __exceptfds,
            TimeH.timeval* __timeout
        );

        public struct fd_set
        {
            public fixed int __fds_bits[FD_SETSIZE / NFDBITS];
        }
    }
}