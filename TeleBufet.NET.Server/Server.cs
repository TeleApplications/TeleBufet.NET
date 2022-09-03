using DatagramsNet;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using TeleBufet.NET.API;
using TeleBufet.NET.API.Database;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;

namespace TeleBufet.NET.Server
{
    internal sealed class Server : SocketServer
    {
        public override Socket CurrentSocket => new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        public override int PortNumber => 1111;

        protected override int bufferSize => 128000;

        private IdentificatorGenerator identificatorGenerator = new();
        private static readonly MethodInfo changeType = typeof(GenericType).GetMethod(nameof(GenericType.ReType));

        public Server(IPAddress address) : base(address)
        {
            CurrentSocket.Bind(EndPoint);
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshake newDatagram) 
            {
                var responseDatagram = new TwoWayHandshakeResponsePacket()
                {
                    IpAddress = IPAddress.ToString()
                };

                //var testIPAddress = IPAddress.Parse()
                var currentIPAddress = ((IPEndPoint)ipAddress);

                await ServerLogger.LogAsync<NormalPrefix>($"Packet recieved from {newDatagram}... sending packet to client", TimeFormat.Half);
                await SendToDatagramAsync(responseDatagram, ipAddress);
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

                    await SendToDatagramAsync(accountInformationPacket, ipAddress);
                }
            }
            
            //TODO: Update products amount
            if (datagram is CacheProductsTablePacket newCacheDatagram)
            {
                UncachedTablesPacket uncachedTables = new();

                var tableType = newCacheDatagram.TableType;
                var tableName = newCacheDatagram.TableType.Name;

                if(tableType == typeof(ProductTable))
                    uncachedTables.TableHolders = CreateTableHolders(await GetNewTables<ProductTable>(newCacheDatagram.CacheTables.ToArray()));
                if(tableType == typeof(CategoryTable))
                    uncachedTables.TableHolders = CreateTableHolders(await GetNewTables<CategoryTable>(newCacheDatagram.CacheTables.ToArray()));
                if(tableType == typeof(ImageTable))
                    uncachedTables.TableHolders = CreateTableHolders(await GetNewTables<ImageTable>(newCacheDatagram.CacheTables.ToArray()));
                //uncachedTables.Categories = await GetNewTables<CategoryTable>(newCacheDatagram.CacheCategories.ToArray());

                if (uncachedTables.TableHolders.Length == 0 || uncachedTables.TableHolders[0] is null) 
                {
                    await ServerLogger.LogAsync<NormalPrefix>($"{tableName} is up to date", TimeFormat.Half);
                    return;
                }
                uncachedTables.TableType = tableType;
                var data = DatagramHelper.WriteDatagram(uncachedTables);
                await SendToDatagramAsync(uncachedTables, ipAddress);

                await ServerLogger.LogAsync<WarningPrefix>($"In caching process was found {uncachedTables.TableHolders.Length} old datas of type {tableName}", TimeFormat.Half);
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

                await SendToDatagramAsync(informationPacket, ipAddress);
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
                        int index = orderPacket.Products[i].Id - 1;
                        int productAmount = orderPacket.Products[i].Amount;
                        var currentTable = tables.Span[index];

                        await ReservateProductAsync(userId, productAmount, currentTable, orderPacket);
                    }
                }

                await SendToDatagramAsync(orderPacket, ipAddress);
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

        private TableByteHolder[] CreateTableHolders<T>(ReadOnlyMemory<T> tables) where T : ICacheTable<TimeSpan>, new() 
        {
            int tableLength = tables.Length;
            var tableHolders = new TableByteHolder[tableLength];

            for (int i = 0; i < tableLength; i++)
            {
                var currentBytes = BinaryHelper.Write(tables.Span[i]);
                tableHolders[i] = new TableByteHolder()
                {
                    TableBytes = currentBytes
                };
            }
            return tableHolders;
        }

        private async Task<ReadOnlyMemory<T>> GetNewTables<T>(CacheConnection[] cacheTables) where T : ICacheTable<TimeSpan>, new()
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
