using DatagramsNet.Logging.Reading;
using System.Net;

namespace TeleBufet.NET.Server
{
    internal class Program 
    {
        private static Server? server;

        private static IdentificatorGenerator idGenerator;

        public static void Main() 
        {
            idGenerator = new();
            //Console.WriteLine("Please enter valid ip address: ");
            //server = new(nameof(Server), IPAddress.Parse(Console.ReadLine()));
            //Task.Run(() => server.StartServerAsync());
            //ReaderManager.StartReading();

            int offset = 255;
            for (int i = 0; i < sizeof(uint); i++)
            {
                Console.Write($"SchoolBreakId:{i} ");
                for (int j = 0; j < 255 * offset; j++)
                {
                    int identificator = idGenerator.GenerateId((byte)(i));
                    Console.Write($" {identificator},");
                }
                Console.WriteLine();
            }
            Console.ReadLine();
        }
    }
}
