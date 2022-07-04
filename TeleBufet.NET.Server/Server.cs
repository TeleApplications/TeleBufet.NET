using DatagramsNet;
using DatagramsNet.Datagram;
using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Prefixes;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Database;
using TeleBufet.NET.API.Database.Tables;

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
                using (var databaseManager = new DatabaseManager<UserTable>()) 
                {
                    var users = await databaseManager.GetTable();
                    var currentUser = users.FirstOrDefault(n => n.Token == newAuthenticationDatagram.Account.Token);
                    if (currentUser is null) 
                    {
                        currentUser = new UserTable()
                        {
                            Email = newAuthenticationDatagram.Account.Username,
                            Token = newAuthenticationDatagram.Account.Token,
                            Karma = 0
                        };
                        await databaseManager.SetTable(currentUser);
                        await ServerLogger.Log<NormalPrefix>($"Welcome {currentUser.Email} in TeleBufet", TimeFormat.HALF);
                    }
                    else
                        await ServerLogger.Log<NormalPrefix>($"Welcome back {currentUser.Email} in TeleBufet", TimeFormat.HALF);
                    //TODO: Send AccountInformationPacket
                }
            }
        }
    }
}
