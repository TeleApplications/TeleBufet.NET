using DatagramsNet;
using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Prefixes;
using Microsoft.Identity.Client;
using System.Net;
using TeleBufet.NET.Server.TeleBufet.NET.Authentification;

namespace TeleBufet.NET.Server
{
    internal sealed class Server : ServerManager
    {
        public override int PortNumber => 1111;

        private IConfidentialClientApplication clientApplication;

        public AuthentificationManager AuthenticationManager { get; set; }

        public Server(string name, IPAddress address) : base(name, address)
        {
            clientApplication = ConfidentialClientApplicationBuilder.Create("9992351a-ca4c-4f01-ac18-f74865c493ba").WithTenantId("ea80bead-34b4-4c9b-9eee-cde4240e98ce").WithClientSecret("d61ad155-e8c1-49f5-8673-bc5073463102").Build();
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
            }
        }
    }
}
