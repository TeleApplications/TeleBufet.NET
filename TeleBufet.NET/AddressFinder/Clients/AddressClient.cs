using DatagramsNet;
using System.Net;
using TeleBufet.NET.API.Packets.ServerSide;

namespace TeleBufet.NET.AddressFinder.Clients
{
    internal sealed class AddressClient : ServerManager
    {
        public override int PortNumber => 1111;

        public static IPAddress NewIPAddress { get; private set; }

        public AddressClient(string name, IPAddress ipAddress) : base(name, ipAddress) 
        {
            this.ServerSocket.Connect((EndPoint)new IPEndPoint(ipAddress, PortNumber));
            this.UdpReciever = new UdpReciever(ServerSocket);
        }

        public override Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshakeResponsePacket responsePacket) 
            {
                NewIPAddress = IPAddress.Parse(responsePacket.IpAddress);
            }

            return Task.CompletedTask;
        }
    }
}
