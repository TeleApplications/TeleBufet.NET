using DatagramsNet;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Database;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;

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

                    var accountInformationPacket = new AccountInformationPacket()
                    {
                        Karma = currentUser.Karma
                    };
                    await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(accountInformationPacket));
                }
            }

            //TODO: Update products amount
            if (datagram is CacheTablesPacket newCacheDatagram)
            {
                UncachedTablesPacket uncachedTables = new();
                uncachedTables.Products = GetNewTables<ProductTable>(newCacheDatagram.CacheProducts.ToArray()).ToArray();
                uncachedTables.Categories = GetNewTables<CategoryTable>(newCacheDatagram.CacheCategories.ToArray()).ToArray();
                await ServerLogger.Log<NormalPrefix>($"In caching process was found {uncachedTables.Products.Length + uncachedTables.Categories.Length} old datas", TimeFormat.HALF);

                //await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(uncachedTables));
            }
        }

        private IEnumerable<T> GetNewTables<T>(CacheConnection<T, TimeSpan>[] cacheTables) where T : ITable, ICache<TimeSpan>, new() 
        {
            using var databaseManager = new DatabaseManager<T>();
            var products = Task.Run(async() => await databaseManager.GetTable()).Result;
            for (int i = 0; i < products.Length; i++)
            {
                var databaseProduct = products.FirstOrDefault(n => n.Id == cacheTables[i].Id);
                if (databaseProduct is not null)
                {
                    if (databaseProduct.Key != cacheTables[i].Key)
                        yield return databaseProduct;
                }
                else
                    yield return databaseProduct;
            }
        }
    } 
}
