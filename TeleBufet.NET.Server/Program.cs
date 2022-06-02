using System;
using System.Net;

namespace TeleBufet.NET.Server
{
    internal class Program 
    {
        private static Server server;

        public static void Main() 
        {
            Console.WriteLine("Please enter valid ip address");
            server = new(nameof(Server), IPAddress.Parse(Console.ReadLine()));
            Task.Run(() => server.StartServer());
            Console.ReadLine();
        }
    }
}
