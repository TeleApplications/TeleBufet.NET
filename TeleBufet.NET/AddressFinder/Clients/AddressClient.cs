using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;

namespace TeleBufet.NET.AddressFinder.Clients
{
    internal sealed class AddressClient : ServerManager
    {
        public override int PortNumber => 1111;

        public static IPAddress NewIPAddress { get; private set; }
        private IPEndPoint currentEndPoint;

        public AddressClient(string name, IPAddress ipAddress) : base(name, ipAddress) 
        {
            currentEndPoint = new IPEndPoint(ipAddress, PortNumber);

            this.ServerSocket.Connect((EndPoint)new IPEndPoint(IPAddress.Any, PortNumber));
            this.ServerSocket.EnableBroadcast = true;
            this.UdpReciever = new UdpReciever(ServerSocket);
        }

        public async Task PingAsync(TwoWayHandshake handshakePacket) 
        {
            await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, currentEndPoint), DatagramHelper.WriteDatagram(handshakePacket));
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
