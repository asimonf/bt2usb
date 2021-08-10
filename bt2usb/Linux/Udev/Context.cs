using System;
using System.Runtime.InteropServices;
using Mono.Unix;
using static bt2usb.Linux.Udev.Global;

namespace bt2usb.Linux.Udev
{
    /// <summary>
    ///     A udev context object.
    /// </summary>
    /// <remarks>
    ///     All functions require a udev context to operate. It is used to track
    ///     library state and link objects together. No global state is used by
    ///     libudev, everything is always linked to a udev context. Furthermore,
    ///     multiple different udev contexts can be used in parallel by multiple
    ///     threads. However, a single context must not be accessed by multiple
    ///     threads in parallel. The caller is responsible for providing suitable
    ///     locking if they intend to use it from multiple threads.
    /// </remarks>
    public sealed class Context : IDisposable
    {
        private IntPtr _handle;

        private Context(IntPtr udev)
        {
            _handle = udev;
            udev_set_userdata(udev, (IntPtr) GCHandle.Alloc(this, GCHandleType.Weak));
        }

        /// <summary>
        ///     Creates a new udev context
        /// </summary>
        public Context() : this(New())
        {
        }

        /// <summary>
        ///     Gets the pointer to the unmanaged udev context.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                if (_handle == IntPtr.Zero) throw new ObjectDisposedException(null);
                return _handle;
            }
        }

        [DllImport(UdevLibraryName, SetLastError = true)]
        private static extern IntPtr udev_new();

        private static IntPtr New()
        {
            var ptr = udev_new();
            if (ptr == IntPtr.Zero) throw new UnixIOException(Marshal.GetLastWin32Error());
            return ptr;
        }

        [DllImport(UdevLibraryName)]
        private static extern void udev_set_userdata(IntPtr udev, IntPtr userdata);

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_get_userdata(IntPtr udev);

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_ref(IntPtr udev);

        /// <summary>
        ///     Gets the managed instance of the udev context.
        /// </summary>
        /// <remarks>
        ///     If a managed context already exists for this instance (and it has
        ///     not been disposed, it will be returned, otherwise a new managed
        ///     instance will be returned.
        /// </remarks>
        internal static Context GetInstance(IntPtr udev)
        {
            var gcHandle = (GCHandle) udev_get_userdata(udev);
            if (gcHandle.IsAllocated)
            {
                if (gcHandle.Target is Context context && context._handle != IntPtr.Zero) return context;
                gcHandle.Free();
            }

            return new Context(udev_ref(udev));
        }

        #region IDisposable Support

        [DllImport(UdevLibraryName)]
        private static extern IntPtr udev_unref(IntPtr udev);

        private void Dispose(bool disposing)
        {
            if (_handle != IntPtr.Zero)
            {
                udev_unref(_handle);
                _handle = IntPtr.Zero;
            }
        }

        /// <inheritdoc />
        ~Context()
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