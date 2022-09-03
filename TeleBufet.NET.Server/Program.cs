using DatagramsNet.Logging.Reading;
using System.Net;

namespace TeleBufet.NET.Server
{
    internal class Program 
    {
        private static Server? server;

        public static void Main() 
        {
            Console.WriteLine("Please enter valid ip address: ");
            server = new(IPAddress.Parse(Console.ReadLine()));
            Task.Run(() => server.StartServerAsync());
            ReaderManager.StartReading();
        }
    }
}
