namespace bt2usb.Server.Model
{
    public class BtDevice
    {
        public string Name { get; set; }
        public string Address { get; set; }
        
        public bool Paired { get; set; }
        public bool Connected { get; set; }
        public bool Trusted { get; set; }
        public string Icon { get; set; }
    }
}