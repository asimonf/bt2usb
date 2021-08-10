using System;
using System.Linq;

namespace bt2usb.Config
{
    public class Device
    {
        public enum TypeEnum
        {
            Keyboard,
            Mouse,
            DS4
        }

        public string Address { get; set; }
        public string Type { get; set; }

        public bool IsValid => Enum.GetNames(typeof(TypeEnum)).Contains(Type);
    }
}