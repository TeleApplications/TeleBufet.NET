using DatagramsNet;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using System.Net;
using TeleBufet.NET.API.Database;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;

namespace TeleBufet.NET.Server
{
    internal sealed class Server : ServerManager
    {
        private IdentificatorGenerator identificatorGenerator = new();

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
                    var currentUser = users.ToArray().FirstOrDefault(n => n.Token == newAuthenticationDatagram.Account.Token);
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
                        Indetificator = currentUser.Id,
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

                var properTables = GetNewProductTables(newAmountDatagram.ProductInformationTables, products);
                var informationPacket = new ProductsInformationPacket()
                {
                    ProductsInfromations = properTables.ToArray()
                };

                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(informationPacket));
            }

            if (datagram is OrderTransmitionPacket orderPacket) 
            {
                using var databaseManager = new DatabaseManager<ProductInformationTable>();
                var tables = await databaseManager.GetTable();

                int userId = orderPacket.Indetifactor;
                orderPacket.Indetifactor = identificatorGenerator.GenerateId((byte)orderPacket.ReservationTimeId);
                orderPacket.StringDateTime = DateTime.Now.ToShortDateString();

                var orders = orderPacket.Products.AsMemory();

                bool amountCheck = TryGetCheckProductAmount(tables, ref orders);
                if (!(amountCheck))
                    orderPacket.Products = orders.ToArray();
                else
                {
                    for (int i = 0; i < orderPacket.Products.Length; i++)
                    {
                        //This is not the best way, how to find a proper table, but this also saves
                        //a lot of database computing
                        int index = orderPacket.Products[i].Id;
                        int productAmount = orderPacket.Products[i].Amount;
                        var currentTable = tables.Span[index];

                        await ReservateProductAsync(userId, productAmount, currentTable, orderPacket);
                    }
                }
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendToAsync(data, System.Net.Sockets.SocketFlags.None, ipAddress), DatagramHelper.WriteDatagram(orderPacket));
            }
        }

        private async Task ReservateProductAsync(int userId, int amount, ProductInformationTable table, OrderTransmitionPacket packetInfromation)
        {
            using var orderDatabaseManager = new DatabaseManager<OrderTable>();
            using var databaseManager = new DatabaseManager<ProductInformationTable>();

            var currentOrder = new OrderTable()
            {
                Id = packetInfromation.Indetifactor,
                UserId = userId,
                Amount = amount,
                ReservedTime = packetInfromation.ReservationTimeId,
                ProductId = table.Id
            };

            await orderDatabaseManager.SetTable(currentOrder);

            table.Amount -= amount;
            await databaseManager.UpdateTable(table, new ProductInformationTable() { Id = table.Id });
            await ServerLogger.LogAsync<WarningPrefix>($"Client reservate {amount} items with id: {table.Id}", TimeFormat.Half);
        }

        private bool TryGetCheckProductAmount(ReadOnlyMemory<ProductInformationTable> tables, ref Memory<ProductHolder> orders)
        {
            Memory<ProductHolder> oldProducts = orders.ToArray();
            int defaultCount = 0;
            for (int i = 0; i < orders.Length; i++)
            {
                var currentOrder = orders.Span[i];
                var currentTable = tables.Span.ToArray().FirstOrDefault(n => n.Id == currentOrder.Id);

                if (currentTable is null) 
                {
                    ServerLogger.LogAsync<ErrorPrefix>($"Product with id: {currentOrder.Id} was not found", TimeFormat.Half);
                    orders.Span[i] = default!;
                    defaultCount++;
                }

                if (currentTable.Amount < currentOrder.Amount) 
                {
                    ServerLogger.LogAsync<ErrorPrefix>($"{currentOrder.Amount} pieces of product with id: {currentOrder.Id} are not availible", TimeFormat.Half);
                    orders.Span[i] = default!;
                    defaultCount++;
                }
            }
            return defaultCount == 0;
        }

        private ReadOnlyMemory<ProductInformationTable> GetNewProductTables(ProductInformationTable[] orignalTables, ReadOnlyMemory<ProductInformationTable> newTables) 
        {
            if (orignalTables.Length == 0)
                return newTables;

            Span<ProductInformationTable> spanTables = new ProductInformationTable[newTables.Length];
            int totalChanges = 0;
            for (int i = 0; i < newTables.Length; i++)
            {
                if (i > (orignalTables.Length - 1) || !(newTables.Span[i].Equals(orignalTables[i]))) 
                {
                    spanTables[totalChanges] = newTables.Span[i];
                    totalChanges++;
                }
            }
            return spanTables[0..totalChanges].ToArray();
        }

        private async Task<T[]> GetNewTables<T>(CacheConnection[] cacheTables) where T : ITable, ICache<TimeSpan>, new()
        {
            var newTables = new List<T>();
            using var databaseManager = new DatabaseManager<T>();
            var products = await databaseManager.GetTable();
            for (int i = 0; i < products.Length; i++)
            {
                var databaseProduct = cacheTables.FirstOrDefault(n => n.Id == products.Span[i].Id);
                if (databaseProduct is not null)
                {
                    if (databaseProduct.Key != products.Span[i].Key)
                        newTables.Add(products.Span[i]);
                }
                else
                    newTables.Add(products.Span[i]);
            }
            return newTables.ToArray();
        }
    } 
}
