using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.HID;
using bt2usb.Linux.Udev;
using Mono.Unix;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace bt2usb
{
    class Program
    {
        private const string TestDocument = @"---
            devices: 
              - address: dc:2c:26:fe:63:d4
                type: Keyboard
              - address: F3:84:6A:4C:E7:D6
                type: Mouse
              - address: 84:17:66:ed:fa:87
                type: DS4
";
        
        static void Main(string[] args)
        {
            var signals = new[] {
                new UnixSignal (Mono.Unix.Native.Signum.SIGINT),
                new UnixSignal (Mono.Unix.Native.Signum.SIGTERM),
            };
            
            Console.WriteLine("Opening Config");
            using var input = new StringReader(TestDocument);
            
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            var config = deserializer.Deserialize<Config.Config>(input);
            config.CleanupInvalids();
            
            Console.WriteLine("Finding Devices according to the config");
            var manager = new DeviceManager(config);
            manager.FindDevices();
            
            Task.Factory.StartNew(() => 
            {
                UnixSignal.WaitAny(signals, -1);
                manager.StopForwarding();
                manager.Dispose();
            });
            
            Console.WriteLine("Starting Forwarding");
            manager.StartForwarding();
            while (manager.IsForwarding)
            {
                Thread.Sleep(1);
            }
        }
    }
}