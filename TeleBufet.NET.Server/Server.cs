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

        public Server(string name, IPAddress address) : base(name, address)
        {
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshake newDatagram) 
            {
                await ServerLogger.LogAsync<NormalPrefix>($"Packet recieved from {ipAddress.AddressFamily}... sending packet to client", TimeFormat.Half);
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(newDatagram));
            }

            if (datagram is AuthentificateAccountPacket newAuthenticationDatagram) 
            {
                await ServerLogger.LogAsync<NormalPrefix>($"Auth packet: from {newAuthenticationDatagram.Account.Username} with token {newAuthenticationDatagram.Account.Token}", TimeFormat.Half);
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
                        await ServerLogger.LogAsync<NormalPrefix>($"Welcome {currentUser.Email} in TeleBufet", TimeFormat.Half);
                    }
                    else
                        await ServerLogger.LogAsync<NormalPrefix>($"Welcome back {currentUser.Email} in TeleBufet", TimeFormat.Half);
                    var accountInformationPacket = new AccountInformationPacket()
                    {
                        Karma = currentUser.Karma
                    };
                    await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(accountInformationPacket));
                }
            }
            
            //TODO: Update products amount
            if (datagram is CacheTablesPacket newCacheDatagram)
            {
                UncachedTablesPacket uncachedTables = new();
                uncachedTables.Products = await GetNewTables<ProductTable>(newCacheDatagram.CacheProducts.ToArray());
                uncachedTables.Categories = await GetNewTables<CategoryTable>(newCacheDatagram.CacheCategories.ToArray());

                await ServerLogger.LogAsync<NormalPrefix>($"In caching process was found {uncachedTables.Products.Length + uncachedTables.Categories.Length} old datas", TimeFormat.Half);
                var data = DatagramHelper.WriteDatagram(uncachedTables);
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), data);
            }

            if (datagram is RequestProductInformationPacket newAmountDatagram) 
            {
                using var databaseManager = new DatabaseManager<ProductInformationTable>();
                var products = await databaseManager.GetTable();
                var informationPacket = new ProductsInformationPacket()
                {
                    ProductsInfromations = products
                };

                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(informationPacket));
            }
        }

        private async Task<T[]> GetNewTables<T>(CacheConnection[] cacheTables) where T : ITable, ICache<TimeSpan>, new()
        {
            var newTables = new List<T>();
            using var databaseManager = new DatabaseManager<T>();
            var products = await databaseManager.GetTable();
            for (int i = 0; i < products.Length; i++)
            {
                var databaseProduct = cacheTables.FirstOrDefault(n => n.Id == products[i].Id);
                if (databaseProduct is not null)
                {
                    if (databaseProduct.Key != cacheTables[i].Key)
                        newTables.Add(products[i]);
                }
                else
                    newTables.Add(products[i]);
            }
            return newTables.ToArray();
        }
    } 
}
