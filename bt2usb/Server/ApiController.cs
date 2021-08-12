using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using bt2usb.Server.Model;
using Butterfly.Web.EmbedIO;
using Butterfly.Web.WebApi;
using Newtonsoft.Json;

namespace bt2usb.Server
{
    public class ApiController : IDisposable
    {
        private const string DbFile = "btmap.json";

        private readonly EmbedIOContext _context;
        private readonly List<string> _deviceMonitorList = new List<string>();
        private readonly BtService _btService;

        private Dictionary<string, BtDeviceType> _deviceMap = new Dictionary<string, BtDeviceType>();

        public ApiController(int port, BtService btService)
        {
            _btService = btService;
            _context = new EmbedIOContext($"http://+:{port}/", "web");
        }

        public delegate void DeviceMapChangeCallback(string address, BtDeviceType type, bool connected);

        public event DeviceMapChangeCallback DeviceMapChanged;

        public void Start(string adapterName = null)
        {
            _context.WebApi.OnGet("/devices",
                async (request, response) => await GetDevices(request, response));
            _context.WebApi.OnGet("/device/{address}",
                async (request, response) => await GetDeviceByAddress(request, response));

            _context.WebApi.OnPost("/device/{address}/connect",
                async (request, response) => await ConnectToDevice(request, response));

            _context.WebApi.OnPost("/device/{address}/disconnect",
                async (request, response) => await DisconnectToDevice(request, response));
            _context.WebApi.OnPost("/device/{address}/pair",
                async (request, response) => await PairToDevice(request, response));
            _context.WebApi.OnPost("/device/{address}/trust",
                async (request, response) => await TrustDevice(request, response));
            _context.WebApi.OnPost("/device/{address}/map",
                async (request, response) => await MapDevice(request, response));
            _context.WebApi.OnPost("/device/{address}/monitor",
                async (request, response) => await MonitorDevice(request, response));

            _context.WebApi.OnPost("/scan",
                async (request, response) => await Scan(request, response));

            _context.Start();
        }

        public void ReloadDb()
        {
            _deviceMap = File.Exists(DbFile)
                ? JsonConvert.DeserializeObject<Dictionary<string, BtDeviceType>>(File.ReadAllText(DbFile))
                : new Dictionary<string, BtDeviceType>();

            foreach (var (address, _) in _deviceMap)
            {
                DoMonitorDevice(address);
            }
        }

        private async Task OperateDevice(IHttpRequest request, IHttpResponse response,
            Func<BtDevice, Task> operationExpr)
        {
            var address = request.PathParams["address"];

            if (string.IsNullOrEmpty(address))
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new
                {
                    error = "Address not supplied"
                });
                return;
            }

            var device = await _btService.GetDeviceByAddress(address);

            if (device == null)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new
                {
                    error = "Device not Found"
                });
            }
            else
            {
                try
                {
                    await operationExpr(device);
                    await response.WriteAsTextAsync(
                        JsonConvert.SerializeObject(device), "application/json"
                    );
                }
                catch (Exception e)
                {
                    response.StatusCode = 500;
                    await response.WriteAsJsonAsync(new
                    {
                        error = e.Message
                    });
                }
            }
        }

        private async Task Scan(IHttpRequest request, IHttpResponse response)
        {
            var newDevices = new List<BtDevice>();

            await _btService.Scan(device => newDevices.Add(device));

            await response.WriteAsTextAsync(
                JsonConvert.SerializeObject(newDevices), "application/json"
            );
        }

        private async Task ConnectToDevice(IHttpRequest request, IHttpResponse response)
        {
            await OperateDevice(request, response,
                async device => await _btService.DeviceAction(device, BtService.DeviceActionEnum.Connect));
        }

        private async Task DisconnectToDevice(IHttpRequest request, IHttpResponse response)
        {
            await OperateDevice(request, response,
                async device => await _btService.DeviceAction(device, BtService.DeviceActionEnum.Disconnect));
        }

        private async Task PairToDevice(IHttpRequest request, IHttpResponse response)
        {
            await OperateDevice(request, response,
                async device => await _btService.DeviceAction(device, BtService.DeviceActionEnum.Pair));
        }

        private async Task TrustDevice(IHttpRequest request, IHttpResponse response)
        {
            await OperateDevice(request, response,
                async device => await _btService.DeviceAction(device, BtService.DeviceActionEnum.Trust));
        }

        private async Task DoMonitorDevice(string address)
        {
            if (_deviceMonitorList.Contains(address)) return;
            
            _deviceMonitorList.Add(address);
            
            await _btService.MonitorDevice(
                address,
                (args) =>
                {
                    Console.WriteLine("Device {0} connected!", address);
                    if (_deviceMap.TryGetValue(address, out var btDeviceType))
                        OnDeviceMapChanged(address, btDeviceType, true);
                    else
                        Console.WriteLine("Device {0} connected but not mapped!!!", address);
                },
                (args =>
                {
                    Console.WriteLine("Device {0} disconnected!", address);
                    if (_deviceMap.TryGetValue(address, out var btDeviceType))
                        OnDeviceMapChanged(address, btDeviceType, false);
                })
            );
        }

        private async Task MapDevice(IHttpRequest request, IHttpResponse response)
        {
            await OperateDevice(request, response, async btDevice =>
            {
                var data = await request.ParseAsJsonAsync<Dictionary<string, string>>();

                if (!data.ContainsKey("deviceType"))
                    throw new Exception("'deviceType' not specified");

                if (!Enum.TryParse<BtDeviceType>(data["deviceType"], out var btDeviceType))
                    throw new Exception("Invalid type selected");

                if (_deviceMap.ContainsKey(btDevice.Address))
                    throw new Exception("Device already bound");

                _deviceMap.Add(btDevice.Address, btDeviceType);
                var address = btDevice.Address;

                await DoMonitorDevice(address);

                // if (btDevice.Connected) OnDeviceMapChanged(btDevice.Address, btDeviceType, true);
            });
        }

        private async Task MonitorDevice(IHttpRequest request, IHttpResponse response)
        {
            var address = request.PathParams["address"].ToUpperInvariant();

            if (string.IsNullOrEmpty(address))
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new
                {
                    error = "Address not supplied"
                });
                return;
            }

            if (_deviceMonitorList.Contains(address))
                throw new Exception("Device already being monitored");
            
            await DoMonitorDevice(address);
            await response.WriteAsJsonAsync(new
            {
                Status = "mapped"
            });
        }

        private async Task GetDeviceByAddress(IHttpRequest request, IHttpResponse response)
        {
            var address = request.PathParams["address"];

            var device = await _btService.GetDeviceByAddress(address);

            if (device == null)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { });
            }
            else
            {
                await response.WriteAsTextAsync(
                    JsonConvert.SerializeObject(device), "application/json"
                );
            }
        }

        private async Task GetDevices(IHttpRequest request, IHttpResponse response)
        {
            var devices = await _btService.GetDevices();

            await response.WriteAsTextAsync(
                JsonConvert.SerializeObject(devices), "application/json"
            );
        }

        public void Dispose()
        {
            File.WriteAllText(DbFile, JsonConvert.SerializeObject(_deviceMap));            

            _context.Dispose();
        }

        protected virtual void OnDeviceMapChanged(string address, BtDeviceType type, bool connected)
        {
            DeviceMapChanged?.Invoke(address, type, connected);
        }
    }
}