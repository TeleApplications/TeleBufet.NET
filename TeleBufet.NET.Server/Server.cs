using DatagramsNet;
using DatagramsNet.Datagrams.NET.Prefixes;
using DatagramsNet.NET.Logger;
using System.Net;

namespace TeleBufet.NET.Server
{
    internal sealed class Server : ServerManager
    {
        public override int PortNumber => 1111;

        public Server(string name, IPAddress address) : base(name, address) {}

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandShakePacket newDatagram) 
            {
                await ServerLogger.Log<NormalPrefix>($"Packet recieved from {ipAddress.AddressFamily}... sending packet to client", TimeFormat.HALF);
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(newDatagram));
            }
        }
    }
}
