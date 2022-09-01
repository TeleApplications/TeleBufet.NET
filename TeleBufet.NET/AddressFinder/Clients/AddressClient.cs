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

        protected override CancellationTokenSource CancellationSource =>
            new CancellationTokenSource();

        public AddressClient(string name, IPAddress ipAddress) : base(name, IPAddress.Any) 
        {
            CancellationSource.Cancel(this.ServerSocket.Connected);

            currentEndPoint = new IPEndPoint(ipAddress, PortNumber);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            ServerSocket.EnableBroadcast = true;
            this.UdpReciever = new UdpReciever(ServerSocket);

            Task.Run(() => this.StartServerAsync());
        }

        public async Task PingAsync(TwoWayHandshake handshakePacket) 
        {
            handshakePacket.Message = IPAddress.ToString();
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

        protected override async Task<ClientDatagram> StartRecievingAsync()
        {
            Memory<byte> datagramMemory = new byte[4096];
            EndPoint currentEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            SocketReceiveFromResult resultData;
            if (ServerSocket.Connected) 
            {
                resultData = await ServerSocket.ReceiveFromAsync(datagramMemory, SocketFlags.None, currentEndPoint);
                return new ClientDatagram() { Client = (IPEndPoint)resultData.RemoteEndPoint, Datagram = datagramMemory.Span.ToArray() };
            }

            return new ClientDatagram();
        }

        public async Task CloseConnectionAsync() 
        {
            ServerSocket.Close();
            ServerSocket.Disconnect(false);
			ServerSocket.Dispose();
        }
    }
}
