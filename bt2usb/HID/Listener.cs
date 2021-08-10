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
        private readonly ConcurrentDictionary<byte, int> _fdDictionary;
        private readonly ConcurrentQueue<(string rawHidPath, byte id)> _hidRawList;
        private readonly TcpListener _inputServer;
        private readonly TcpListener _outputServer;
        private readonly object _syncRoot = new object();
        private readonly ConcurrentBag<Task> _tasks;

        private TcpClient _inputClient;
        private NetworkStream _inputStream;
        private TcpClient _outputClient;
        private NetworkStream _outputStream;

        private bool _serverStarted;

        public Listener()
        {
            _inputServer = new TcpListener(IPAddress.Any, 32100);
            _outputServer = new TcpListener(IPAddress.Any, 32101);
            _hidRawList = new ConcurrentQueue<(string rawHidPath, byte id)>();
            _tasks = new ConcurrentBag<Task>();
            _fdDictionary = new ConcurrentDictionary<byte, int>();
        }

        private void AcceptInputClient(IAsyncResult ar)
        {
            var client = _inputServer.EndAcceptTcpClient(ar);

            if (_inputClient is {Connected: true})
            {
                Console.WriteLine("Already has a client. Rejecting!");
                client.Close();
            }
            else
            {
                Console.WriteLine("Accepting Connection");
                _inputClient = client;
                _inputStream = _inputClient.GetStream();
            }

            _inputServer.BeginAcceptTcpClient(AcceptInputClient, null);
        }

        private void AcceptOutputClient(IAsyncResult ar)
        {
            var client = _outputServer.EndAcceptTcpClient(ar);

            if (_outputClient is {Connected: true})
            {
                Console.WriteLine("Already has a client. Rejecting!");
                client.Close();
            }
            else
            {
                Console.WriteLine("Accepting Connection");
                _outputClient = client;
                _outputStream = _outputClient.GetStream();
            }

            _outputServer.BeginAcceptTcpClient(AcceptInputClient, null);
        }

        public void Start()
        {
            if (_serverStarted) return;
            _serverStarted = true;

            _tasks.Add(Task.Factory.StartNew(ProcessReport));
            while (_hidRawList.TryDequeue(out var tuple))
                _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(tuple.rawHidPath, tuple.id)));

            _inputServer.Start();
            _inputServer.BeginAcceptTcpClient(AcceptInputClient, null);

            _outputServer.Start();
            _outputServer.BeginAcceptTcpClient(AcceptOutputClient, null);
        }

        public void Stop()
        {
            if (_serverStarted)
            {
                _inputServer.Stop();
                _outputServer.Stop();
                _serverStarted = false;
            }

            foreach (var task in _tasks)
            {
                task.Wait();
                task.Dispose();
            }

            _tasks.Clear();

            if (_inputClient != null)
            {
                _inputClient.Close();
                _inputClient = null;
                _inputStream = null;
            }

            if (_outputClient != null)
            {
                _outputClient.Close();
                _outputClient = null;
                _outputStream = null;
            }
        }

        public void AddController(string rawHidPath, byte id)
        {
            if (_serverStarted)
                _tasks.Add(Task.Factory.StartNew(() => ForwardLoop(rawHidPath, id)));
            else
                _hidRawList.Enqueue((rawHidPath, id));
        }

        private unsafe void ProcessReport()
        {
            const int size = 79;
            var buf = stackalloc byte[size];
            var bufSpan = new Span<byte>(buf, size);

            while (_serverStarted)
            {
                bufSpan.Fill(0);

                if (_outputClient != null && _outputStream != null)
                {
                    var len = _outputStream.Read(bufSpan);

                    if (len != size)
                    {
                        Console.WriteLine("Mismatch {0}", len);
                        continue;
                    }

                    var id = buf[len - 1];

                    if (!_fdDictionary.ContainsKey(id))
                    {
                        Console.WriteLine("Controller not found {0}", id);
                        continue;
                    }

                    var fd = _fdDictionary[id];

                    lock (_syncRoot)
                    {
                        var ret = Syscall.write(fd, buf, size - 1);
                        if (ret < 0)
                        {
                            Console.WriteLine("Error writing to FD");
                            var errno = Stdlib.GetLastError();
                            Console.WriteLine("{0}", errno);

                            var data = BitConverter.ToString(bufSpan.ToArray());
                            Console.WriteLine(data);
                        }
                    }
                }

                Thread.Yield();
            }
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

            _fdDictionary.TryAdd(id, rawFd);

            var pollArr = new[]
            {
                new Pollfd
                {
                    fd = rawFd,
                    events = PollEvents.POLLIN | PollEvents.POLLERR | PollEvents.POLLHUP
                }
            };

            while (_serverStarted)
            {
                var ret = Syscall.poll(pollArr, 1000);
                if (ret == -1)
                {
                    Console.WriteLine("Error controller poll");
                    return;
                }

                if (ret == 0) continue;

                if ((pollArr[0].revents & PollEvents.POLLIN) != 0)
                {
                    lock (_syncRoot)
                    {
                        var len = Syscall.read(rawFd, buf + 1, 126);
                        if (len < 0)
                        {
                            Console.WriteLine("Error read");
                            return;
                        }

                        if (len == 0) continue;

                        buf[0] = (byte) (len + 1);
                        buf[len + 1] = id;

                        var bufSpan = new ReadOnlySpan<byte>(buf, (int) len + 2);

                        if (_inputClient != null && _inputStream != null)
                            try
                            {
                                lock (_inputStream)
                                {
                                    _inputStream.Write(bufSpan);
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Error writing to stream");
                                _inputStream.Dispose();
                                _inputClient.Dispose();
                                _inputStream = null;
                                _inputClient = null;
                            }
                    }
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

            Console.WriteLine("Closing forwarder for id: {0}", id);
        }
    }
}