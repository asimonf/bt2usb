using System.Collections.Generic;

namespace bt2usb.Config
{
    public class Config
    {
        public List<Device> Devices { get; set; }

        public void CleanupInvalids()
        {
            Devices.RemoveAll(device => !device.IsValid);
        }
    }
}