using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix.Native;

namespace bt2usb.HID
{
    public class Listener
    {
        private readonly TcpListener _server;
        private readonly ConcurrentQueue<(string rawHidPath, byte id)> _hidRawList;
        private readonly ConcurrentBag<Task> _tasks;

        private bool _serverStarted;

        private TcpClient _client;
        private NetworkStream _clientStream;

        public Listener()
        {
            _server = new TcpListener(IPAddress.Any, 32100);
            _hidRawList = new ConcurrentQueue<(string rawHidPath, byte id)>();
            _tasks = new ConcurrentBag<Task>();
        }

        public void Start()
        {
            if (_serverStarted) return;
            _serverStarted = true;

            _server.Start();
            _server.BeginAcceptTcpClient((ar =>
            {
                _client = _server.EndAcceptTcpClient(ar);
                _clientStream = _client.GetStream();
                _server.Stop();
                _serverStarted = false;

                while (_hidRawList.TryDequeue(out var tuple))
                {
                    _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(tuple.rawHidPath, tuple.id)));
                }
            }), null);
        }

        public void Stop()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
                _clientStream = null;
            }

            if (_serverStarted)
                _server.Stop();
            
            foreach (var task in _tasks)
            {
                task.Wait();
                task.Dispose();
            }

            _tasks.Clear();
        }

        public void AddController(string rawHidPath, byte id)
        {
            if (_client is {Connected: true})
            {
                _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(rawHidPath, id)));
            }
            else
                _hidRawList.Enqueue((rawHidPath, id));
        }

        private unsafe void ForwardLoop(string path, byte id)
        {
            var buf = stackalloc byte[128];
            buf[0] = id;
            var rawFd = Syscall.open(path, OpenFlags.O_RDWR | OpenFlags.O_EXCL);
            if (rawFd < 0)
            {
                Console.WriteLine("Error opening exclusive");
                return;
            }
            var pollArr = new[]
            {
                new Pollfd
                {
                    fd = rawFd,
                    events = PollEvents.POLLIN | PollEvents.POLLERR | PollEvents.POLLHUP
                }
            };

            while (_client is {Connected: true})
            {
                var ret = Syscall.poll(pollArr, 1000);
                if (ret == -1)
                {
                    Console.WriteLine("Error select");
                    return;
                } 
                
                if (ret == 0) continue;
                
                if ((pollArr[0].revents & PollEvents.POLLIN) != 0)
                {
                    var len = Syscall.read(rawFd, &buf[1], 127);
                    if (len < 0)
                    {
                        Console.WriteLine("Error read");
                        return;
                    }

                    var bufSpan = new ReadOnlySpan<byte>(buf, (int) len);
                    _clientStream.Write(bufSpan);
                }
                else if (
                    (pollArr[0].revents & PollEvents.POLLERR) > 0 ||
                    (pollArr[0].revents & PollEvents.POLLHUP) > 0
                )
                {
                    Console.WriteLine("Controller HUP");
                    return;
                }

                Thread.Yield();
            }
        }
    }
}