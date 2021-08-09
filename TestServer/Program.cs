using System;
using System.Net;
using System.Net.Sockets;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TcpClient();
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.7.2"), 32100);
            client.Connect(endpoint);

            var stream = client.GetStream();

            var buffer = new byte[128];
            
            while (client.Connected)
            {
                var len = stream.Read(buffer);
                
                if (len <= 0) continue;

                var data = BitConverter.ToString(buffer, 0, len);
                
                Console.WriteLine(data);
            }
        }
    }
}