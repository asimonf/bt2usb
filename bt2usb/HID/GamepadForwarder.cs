using System;
using System.Collections.Generic;
using System.Linq;
using bt2usb.Linux.Udev;
using bt2usb.Server.Model;

namespace bt2usb.HID
{
    public class GamepadForwarder: IDisposable
    {
        private readonly Listener _listener = new Listener();
        
        private readonly Dictionary<string, byte> _gamepadIdDictionary = new Dictionary<string, byte>();

        public void Start()
        {
            _listener.Start();
        }
        
        public void Dispose()
        {
            _listener.Dispose();
        }

        public void AddDeviceMap(string uniq, string hidRawDevNode)
        {
            byte id;
            if (_gamepadIdDictionary.ContainsKey(uniq))
            {
                id = _gamepadIdDictionary[uniq];
            }
            else
            {
                // Find first empty ID from 0 to 7.
                var ids = _gamepadIdDictionary.Values.ToArray();

                id = 0;
                for (byte i = 0; i < 8; i++)
                {
                    if (ids.Contains(i)) continue;
                    id = i;
                    break;
                }
                
                _gamepadIdDictionary.Add(uniq, id);
            }

            Console.WriteLine("addr: {0} -> id: {1}", uniq, id);
            _listener.AddController(hidRawDevNode, id);
        }

        public void DeleteDeviceMap(string uniq)
        {
            if (!_gamepadIdDictionary.ContainsKey(uniq)) return;
            
            var id = _gamepadIdDictionary[uniq];

            _listener.RemoveController(id);
        }
    }
}