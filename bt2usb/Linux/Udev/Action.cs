namespace bt2usb.Linux.Udev
{
    /// <summary>
    ///     Strings that may be returned by <see cref="Device.Action" />.
    /// </summary>
    public static class Action
    {
        /// <summary>
        ///     The device was added
        /// </summary>
        public const string Add = "add";

        /// <summary>
        ///     The device was removed
        /// </summary>
        public const string Remove = "remove";

        /// <summary>
        ///     The device has changed
        /// </summary>
        public const string Change = "change";
    }
}