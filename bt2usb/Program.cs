using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.HID;
using bt2usb.Linux.Udev;
using Mono.Unix;
using Mono.Unix.Native;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace bt2usb
{
    internal class Program
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

        private static void Main(string[] args)
        {
            var signals = new[]
            {
                new UnixSignal(Signum.SIGINT),
                new UnixSignal(Signum.SIGTERM)
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
            manager.Setup();

            Task.Factory.StartNew(() =>
            {
                UnixSignal.WaitAny(signals, -1);
                manager.StopForwarding();
                manager.Dispose();
            });

            Console.WriteLine("Starting Forwarding");
            manager.StartForwarding();
            while (manager.IsForwarding) Thread.Sleep(1);
        }

        public static void DescribeDevice(Device device)
        {
            Console.WriteLine("--- START ---");

            try
            {
                Console.WriteLine("Driver {0}", device.Driver);
            }
            catch
            {
                Console.WriteLine("No driver");
            }

            Console.WriteLine("Subsystem {0}", device.Subsystem);
            Console.WriteLine("DevPath {0}", device.DevPath);

            try
            {
                foreach (var link in device.Tags) Console.WriteLine("Tag {0}", link);
            }
            catch
            {
                Console.WriteLine("No tags");
            }

            try
            {
                foreach (var link in device.AttributeNames)
                {
                    var value = "No value";

                    try
                    {
                        var ret = device.TryGetAttribute(link);

                        value = link == "report_descriptor"
                            ? BitConverter.ToString(ret)
                            : Encoding.ASCII.GetString(ret);
                    }
                    catch
                    {
                        // ignored
                    }

                    if (link == "report_descriptor")
                    {
                    }

                    Console.WriteLine("Attribute {0}: {1}", link, value);
                }
            }
            catch
            {
                Console.WriteLine("No attributes");
            }

            try
            {
                foreach (var link in device.DevLinks) Console.WriteLine("Tag {0}", link);
            }
            catch
            {
                Console.WriteLine("No links");
            }

            Console.WriteLine("DevNode {0}", device.DevNode);
            Console.WriteLine("SysName {0}", device.SysName);

            foreach (var (key, value) in device.Properties) Console.WriteLine("Property {0}: {1}", key, value);

            Console.WriteLine("SysPath {0}", device.SysPath);
            Console.WriteLine("--- END ---");
            Console.WriteLine();
        }
    }
}