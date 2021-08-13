﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix.Native;

namespace bt2usb.HID
{
    public class Listener: IDisposable
    {
        private readonly ConcurrentDictionary<byte, int> _fdDictionary;
        private readonly TcpListener _server;
        private readonly ConcurrentDictionary<byte, (CancellationTokenSource, Task)> _tasks;
        
        private readonly object _syncRoot = new object();

        private TcpClient _client;
        private NetworkStream _stream;

        private bool _serverStarted;

        public Listener()
        {
            _server = new TcpListener(IPAddress.Any, 32100);
            _tasks = new ConcurrentDictionary<byte, (CancellationTokenSource, Task)>();
            _fdDictionary = new ConcurrentDictionary<byte, int>();
        }

        private void AcceptClient(IAsyncResult ar)
        {
            if (!_serverStarted) return;
            
            var client = _server.EndAcceptTcpClient(ar);

            if (_client != null && _stream != null)
            {
                Console.WriteLine("Already has a client. Closing!");
                _client.Close();
                _client.Dispose();
            }
            
            Console.WriteLine("Accepting Connection");
            _client = client;
            _stream = client.GetStream();

            _server.BeginAcceptTcpClient(AcceptClient, null);
        }

        public void Start()
        {
            if (_serverStarted) return;
            _serverStarted = true;

            var source = new CancellationTokenSource();
            
            _tasks.TryAdd(0xff, (source, Task.Factory.StartNew(() => ProcessReport(source.Token))));

            _server.Start();
            _server.BeginAcceptTcpClient(AcceptClient, null);
        }

        public void Dispose()
        {
            _serverStarted = false;
            _server.Stop();
                
            Console.WriteLine("Server Stopped");

            foreach (var task in _tasks)
            {
                task.Value.Item1.Cancel();
                task.Value.Item2.Wait();
                task.Value.Item1.Dispose();
                task.Value.Item2.Dispose();
            }

            Console.WriteLine("GamepadForwarder Tasks Stopped");
            
            _tasks.Clear();

            _stream?.Dispose();
            _client?.Dispose();
            
            Console.WriteLine("GamepadForwarder Disposed");
        }

        public void AddController(string rawHidPath, byte id)
        {
            var source = new CancellationTokenSource();
            
            RemoveController(id);
            
            Console.WriteLine("Creating tasks for forwarding loop for controller");
            _tasks.TryAdd(id, (source, Task.Factory.StartNew(() => ForwardLoop(rawHidPath, id, source.Token))));
        }

        private unsafe void ProcessReport(CancellationToken cancellationToken)
        {
            var buf = stackalloc byte[400];
            var bufSpanRoot = new Span<byte>(buf, 400);
            
            Console.WriteLine("Processing reports");

            while (!cancellationToken.IsCancellationRequested)
            {
                bufSpanRoot.Fill(0);

                Span<byte> bufSpan;
                
                try
                {
                    var res = _client?.Client.Poll(1000, SelectMode.SelectRead) ?? false;

                    if (!res) continue;

                    var size = -1;

                    var reportId = _stream?.ReadByte() ?? -1;
                    if (reportId < 0)
                    {
                        Console.WriteLine("Error reading report id");
                        continue;
                    }

                    buf[0] = (byte) reportId; 
                    
                    size = reportId switch
                    {
                        0x11 => 78,
                        0x15 => 334,
                        _ => size
                    };

                    if (size < 0)
                    {
                        Console.WriteLine("Unrecognized reportId");
                        continue;
                    }
                    
                    bufSpan = bufSpanRoot.Slice(1, size);
                    
                    var bytesReadSoFar = 0;
                    while (bytesReadSoFar < size)
                    {
                        var bytesRead = _stream?.Read(bufSpan.Slice(bytesReadSoFar, size - bytesReadSoFar)) ?? -1;
                        if (bytesRead <= 0) break;

                        bytesReadSoFar += bytesRead;
                    }
                
                    if (bytesReadSoFar != size)
                    {
                        Console.WriteLine("Houston, we've got a problem... {0}, {1}", size, bytesReadSoFar);
                        continue;
                    }
                    
                    var id = buf[size + 1];
                    
                    if (!_fdDictionary.ContainsKey(id))
                    {
                        Console.WriteLine("Controller not found {0}", id);
                        continue;
                    }

                    var fd = _fdDictionary[id];

                    lock (_syncRoot)
                    {
                        var ret = Syscall.write(fd, buf, (ulong)(size));
                        if (ret < 0)
                        {
                            Console.WriteLine("Error writing to FD for gamepad");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    lock (_syncRoot)
                    {
                        _stream?.Dispose();
                        _client?.Dispose();
                        _stream = null;
                        _client = null;                        
                    }
                }
                finally
                {
                    Thread.Yield();                    
                }
            }
            
            Console.WriteLine("Done processing reports");
        }

        private unsafe void ForwardLoop(string path, byte id, CancellationToken cancellationTokenSource)
        {
            var maxSize = 1024;
            var buf = stackalloc byte[maxSize];
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

            try
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    var ret = Syscall.poll(pollArr, 1000);
                    if (ret == -1)
                    {
                        Console.WriteLine("Error controller poll");
                        return;
                    }
                    else if (ret == 0) continue;

                    if ((pollArr[0].revents & PollEvents.POLLIN) != 0)
                    {
                        lock (_syncRoot)
                        {
                            var len = Syscall.read(rawFd, buf, (ulong)maxSize - 1);
                            if (len < 0)
                            {
                                Console.WriteLine("Error read");
                                return;
                            }

                            if (len == 0) continue;

                            buf[78] = id;

                            var bufSpan = new ReadOnlySpan<byte>(buf, 79);
                            
                            // if (len > 50)
                            // {
                            //     Console.WriteLine("packet ({1}): {0}", BitConverter.ToString(bufSpan.ToArray()), len);
                            //     continue;
                            // }

                            if (_client != null && _stream != null)
                            {
                                try
                                {
                                    lock (_syncRoot)
                                    {
                                        _stream.Write(bufSpan);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Error writing to stream");
                                    Console.WriteLine(e.Message);
                                    _stream.Dispose();
                                    _client.Dispose();
                                    _stream = null;
                                    _client = null;
                                    continue;
                                }
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

                    Task.Yield();
                }
            }
            finally
            {
                _fdDictionary.TryRemove(id, out _);
                Console.WriteLine("Closing FD");
                Syscall.close(rawFd);
            }
        }

        public void RemoveController(byte id)
        {
            Console.Write("Trying to remove controller... ");
            if (!_tasks.TryRemove(id, out var item))
            {
                Console.WriteLine("Not found.");
                return;
            }

            if (!item.Item2.IsCompleted)
            {
                item.Item1.Cancel();
                item.Item2.Wait();                
            }
            
            item.Item1.Dispose();
            item.Item2.Dispose();
            
            Console.WriteLine("Removed!");
        }
    }
}