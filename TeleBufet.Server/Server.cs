using DatagramsNet;
using DatagramsNet.Datagrams.NET.Prefixes;
using DatagramsNet.NET.Logger;
using System.Net;

namespace TeleBufet.Server
{
    internal sealed class Server : ServerManager
    {
        public override int PortNumber => 1111;

        public ServerSender sender { get; private set; }

        public Server(string name, IPAddress address) : base(name, address) { sender = new ServerSender(nameof(Server), PortNumber); }

        public override async Task OnRecieveAsync(object datagram, IPAddress ipAddress)
        {
            if (datagram is HandShakePacket newDatagram) 
            {
                await ServerLogger.Log<NormalPrefix>("Packet recieved... sending packet to client", TimeFormat.HALF);
                await sender.SendDatagramAsync(ipAddress, newDatagram);
            }
        }
    }
}
