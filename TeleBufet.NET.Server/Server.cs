using DatagramsNet;
using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Prefixes;
using Microsoft.Identity.Client;
using System.Net;
using System.Net.Sockets;

namespace TeleBufet.NET.Server
{
    internal sealed class Server : ServerManager
    {
        public override int PortNumber => 1111;

        public static Socket ?ServerSocket { get; private set; }

        public Server(string name, IPAddress address) : base(name, address)
        {
            ServerSocket = serverSocket;
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandShakePacket newDatagram) 
            {
                await ServerLogger.Log<NormalPrefix>($"Packet recieved from {ipAddress.AddressFamily}... sending packet to client", TimeFormat.HALF);
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(newDatagram));
            }

            if (datagram is AuthentificateAccountPacket newAuthenticationDatagram) 
            {
                await ServerLogger.Log<NormalPrefix>($"Auth packet: from {newAuthenticationDatagram.Account.Username} with token {newAuthenticationDatagram.Account.Token}", TimeFormat.HALF);
            }
        }
    }
}
