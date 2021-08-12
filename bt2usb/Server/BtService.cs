using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bt2usb.Server.Model;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

namespace bt2usb.Server
{
    public class BtService
    {
        public enum DeviceActionEnum
        {
            Connect,
            Trust,
            Pair,
            Disconnect
        }

        private class Message : IDisposable
        {
            public readonly ManualResetEventSlim Signal = new ManualResetEventSlim();

            public void Dispose()
            {
                Signal.Dispose();
            }
        }

        private class MonitorDeviceMessage : Message
        {
            public string Address { get; set; }
            
            public DeviceEventHandlerAsync Connected { get; set; }
            public DeviceEventHandlerAsync Disconnected { get; set; }
        }
        
        private class GetDeviceByAddressMessage : Message
        {
            public string Address { get; set; }

            public BtDevice Device { get; set; }
        }

        private class GetDevicesMessage : Message
        {
            public readonly IList<BtDevice> DeviceList = new List<BtDevice>();
        }

        private class ScanMessage : Message
        {
            public Action<BtDevice> OnDevicesAdded { get; set; }
        }

        private class DeviceActionMessage : Message
        {
            public string Address { get; set; }
            public DeviceActionEnum Action { get; set; }
        }

        private readonly ConcurrentQueue<Message> _messageQueue = new ConcurrentQueue<Message>();
        
        private IAdapter1 _adapter;

        public async Task Setup(string adapterName = null)
        {
            if (null != adapterName)
            {
                _adapter = await BlueZManager.GetAdapterAsync(adapterName);
            }
            else
            {
                var adapters = await BlueZManager.GetAdaptersAsync();
                if (adapters.Count == 0)
                {
                    throw new Exception("No Bluetooth adapters found.");
                }

                _adapter = adapters.First();
            }
        }

        private static async Task<BtDevice> CreateBtDevice(Device device)
        {
            var properties = await device.GetAllAsync();

            return new BtDevice()
            {
                Icon = properties.Icon,
                Address = properties.Address.ToUpperInvariant(),
                Name = properties.Name,
                Connected = properties.Connected,
                Paired = properties.Paired,
                Trusted = properties.Trusted
            };
        }

        public async Task<BtDevice> GetDeviceByAddress(string address)
        {
            var message = new GetDeviceByAddressMessage()
            {
                Address = address
            }; 
            _messageQueue.Enqueue(message);

            await Task.Factory.StartNew(() => message.Signal.Wait());

            return message.Device;
        }

        public async Task DeviceAction(BtDevice device, DeviceActionEnum action)
        {
            var message = new DeviceActionMessage
            {
                Address = device.Address,
                Action = action,
            };
            
            _messageQueue.Enqueue(message);
            await Task.Factory.StartNew(() => message.Signal.Wait());
        }

        public async Task<IList<BtDevice>> GetDevices()
        {
            var message = new GetDevicesMessage();
            
            _messageQueue.Enqueue(message);
            await Task.Factory.StartNew(() => message.Signal.Wait());
            
            return message.DeviceList;
        }

        public async Task Scan(Action<BtDevice> action)
        {
            var message = new ScanMessage
            {
                OnDevicesAdded = action
            }; 
            _messageQueue.Enqueue(message);

            await Task.Factory.StartNew(() => message.Signal.Wait());
        }

        public async Task MonitorDevice(string address, Action<BlueZEventArgs> connected, Action<BlueZEventArgs> disconnected)
        {
            var message = new MonitorDeviceMessage
            {
                Address = address,
                Connected = async (_, args) => connected(args),
                Disconnected = async (_, args) => disconnected(args),
                
            }; 
            _messageQueue.Enqueue(message);

            await Task.Factory.StartNew(() => message.Signal.Wait());
        }

        public async Task ProcessMessage()
        {
            while (_messageQueue.TryDequeue(out var message))
            {
                switch (message)
                {
                    case GetDevicesMessage devicesMessage:
                        var deviceList = await _adapter.GetDevicesAsync();
                        foreach (var deviceItem in deviceList)
                        {
                            devicesMessage.DeviceList.Add(await CreateBtDevice(deviceItem));
                        }
                        break;
                    case GetDeviceByAddressMessage getDeviceByAddressMessage:
                        getDeviceByAddressMessage.Device =
                            await CreateBtDevice(await _adapter.GetDeviceAsync(getDeviceByAddressMessage.Address));
                        break;
                    case ScanMessage scanMessage:
                        using (await _adapter.WatchDevicesAddedAsync(
                            async dev => { scanMessage.OnDevicesAdded(await CreateBtDevice(dev)); })
                        )
                        {
                            await _adapter.StartDiscoveryAsync();
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            await _adapter.StopDiscoveryAsync();
                        }
                        break;
                    case DeviceActionMessage actionMessage:
                        var device = await _adapter.GetDeviceAsync(actionMessage.Address);

                        switch (actionMessage.Action)
                        {
                            case DeviceActionEnum.Connect:
                                await device.ConnectAsync();
                                await device.WaitForPropertyValueAsync("Connected", value: true, TimeSpan.FromSeconds(15));
                                await device.WaitForPropertyValueAsync("ServicesResolved", value: true, TimeSpan.FromSeconds(15));
                                break;
                            case DeviceActionEnum.Trust:
                                await device.SetTrustedAsync(true);
                                break;
                            case DeviceActionEnum.Pair:
                                await device.PairAsync();
                                break;
                            case DeviceActionEnum.Disconnect:
                                await device.DisconnectAsync();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    case MonitorDeviceMessage monitorDeviceMessage:
                        var monitorDevice = await _adapter.GetDeviceAsync(monitorDeviceMessage.Address);
                        monitorDevice.Connected += monitorDeviceMessage.Connected;
                        monitorDevice.Disconnected += monitorDeviceMessage.Disconnected;
                        break;
                }

                message.Signal.Set();
                await Task.Yield();
            }
        }
    }
}