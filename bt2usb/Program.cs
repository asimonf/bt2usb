using System;
using System.Linq;
using System.Threading.Tasks;
using bt2usb.HID;
using bt2usb.Server;
using Mono.Unix;
using Mono.Unix.Native;

namespace bt2usb
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var running = true;
            var btService = new BtService();
            await btService.Setup();
            
            var apiController = new ApiController(8000, btService);
            apiController.Start();
            apiController.ReloadDb();

            var deviceManager = new DeviceManager();
            deviceManager.Setup();
            
            var onMapChanged = new ApiController.DeviceMapChangeCallback(deviceManager.NewBtMap);
            
            apiController.DeviceMapChanged += onMapChanged;

            var signals = new[]
            {
                new UnixSignal(Signum.SIGINT),
                new UnixSignal(Signum.SIGTERM)
            };

            Console.WriteLine("Waiting for events");
            while (running)
            {
                await btService.ProcessMessage();

                if (signals.Any(signal => signal.IsSet))
                {
                    running = false;
                }

                await Task.Yield();
            }

            apiController.DeviceMapChanged -= onMapChanged;
            
            deviceManager.Dispose();
            apiController.Dispose();
        }
    }
}