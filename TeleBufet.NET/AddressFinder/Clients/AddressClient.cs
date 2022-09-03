using DatagramsNet;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;

namespace TeleBufet.NET.AddressFinder.Clients
{

    internal sealed class AddressClient : SocketServer
    {
        private bool isClosed = false;

        public override Socket CurrentSocket { get; set; } = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public override int PortNumber => 1111;

        protected override int bufferSize => 128000;

        public static IPAddress NewIPAddress { get; private set; }
        private IPEndPoint currentEndPoint;

        public override Func<bool> CancellationFunction { get; set; }


        public AddressClient(IPAddress ipAddress) : base(IPAddress.Any) 
        {
            currentEndPoint = new IPEndPoint(ipAddress, PortNumber);
            CurrentSocket.Bind(EndPoint);

            SocketReciever = new SocketReciever(CurrentSocket, bufferSize);
            CancellationFunction = () => isClosed; 
            Task.Run(() => this.StartServerAsync());
        }

        public async Task PingAsync(TwoWayHandshake handshakePacket) 
        {
            handshakePacket.Message = IPAddress.ToString();
            await SendToDatagramAsync(handshakePacket, currentEndPoint);
        }

        public override Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshakeResponsePacket responsePacket) 
            {
                NewIPAddress = IPAddress.Parse(responsePacket.IpAddress);
            }

            return Task.CompletedTask;
        }

        public async Task CloseConnectionAsync() 
        {
            isClosed = true;
        }
    }
}
