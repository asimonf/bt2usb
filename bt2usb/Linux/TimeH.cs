// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace bt2usb.Linux
{
    public static class TimeH
    {
        public struct timespec
        {
            public int tv_sec; /* seconds */
            public int tv_nsec; /* nanoseconds */
        }

        public struct timeval
        {
            public int tv_sec; /* seconds */
            public int tv_usec; /* microseconds */
        }
    }
}