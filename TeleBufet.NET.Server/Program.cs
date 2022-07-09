using DatagramsNet.Logging.Reading;
using System;
using System.Net;

namespace TeleBufet.NET.Server
{
    internal class Program 
    {
        private static Server server;

        private static ReaderManager reader;

        public static void Main() 
        {
            server = new(nameof(Server), IPAddress.Parse(Console.ReadLine()));
            reader = new();

            Console.WriteLine("Please enter valid ip address");
            Task.Run(() => server.StartServer());
            reader.StartReading();
        }
    }
}
