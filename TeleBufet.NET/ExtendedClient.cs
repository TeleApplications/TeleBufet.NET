using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.Pages.ProductPage;

namespace TeleBufet.NET
{
    internal sealed class ExtendedClient : ServerManager
    {
        public override int PortNumber => 1111;

        private static ExtendedClient staticHolder;

        private static readonly MethodInfo createConnectioKeys = typeof(TableCacheBuilder).GetMethod(nameof(TableCacheBuilder.GetCacheConnectionKeys));
        private static readonly MethodInfo createCacheTables = typeof(TableCacheBuilder).GetMethod(nameof(TableCacheBuilder.CacheTables));
        private static readonly MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read));
        private static readonly MethodInfo create = typeof(ExtendedClient).GetMethod(nameof(ExtendedClient.CreateTables));

        public ExtendedClient(string name, IPAddress clientAddress) : base(name, IPAddress.Any)
        {
            this.ServerSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(ServerSocket);
            staticHolder = this;
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is AccountInformationPacket newAccountPacket) 
            {
                MainProductPage.User = new UserTable() {Id = newAccountPacket.Indetificator, Karma = newAccountPacket.Karma};
            }

            if (datagram is UncachedTablesPacket newUncachedTablesPacket)
            {
                var tableType = newUncachedTablesPacket.TableType;
                var tableArrayType = Array.CreateInstance(tableType, 1).GetType();

                if (newUncachedTablesPacket.TableHolders is not null) 
                {
                    var holders = newUncachedTablesPacket.TableHolders;
                    var tables = create.MakeGenericMethod(tableType).Invoke(null, new object[] { holders });
                    createCacheTables.MakeGenericMethod(tableType).Invoke(null, new object[] { tables });
                }
            }

            if (datagram is ProductsInformationPacket newProductsInformationPacket) 
            {
                TableCacheBuilder.CacheTables<ProductInformationTable>(newProductsInformationPacket.ProductsInfromations);
            }

            if (datagram is OrderTransmitionPacket newTransmitionPacket) 
            {
                using var productCacheManager = new TableCacheHelper<ProductTable>();
                using var cartCacheHelper = new CartCacheHelper();

                var products = productCacheManager.Deserialize();
                if (TryGetDefaultOrders(out ProductHolder[] defaultProducts, newTransmitionPacket.Products))
                {
                    for (int i = 0; i < defaultProducts.Length; i++)
                    {
                        int index = defaultProducts[i].Id;
                        Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Out of stock", $"Sorry, but product {products[index].Name} is already", "Ok"));

                        cartCacheHelper.CacheValue = new ProductHolder(index, 0);
                        cartCacheHelper.Serialize();
                    }
                }
                else 
                {
                    using var ticketCacheHelperSerialization = new CacheHelper<TicketHolder>();

                    ticketCacheHelperSerialization.CacheValue = new TicketHolder(newTransmitionPacket.Indetifactor, newTransmitionPacket.Products, newTransmitionPacket.ReservationTimeId, newTransmitionPacket.TotalPrice, newTransmitionPacket.StringDateTime);
                    ticketCacheHelperSerialization.Serialize();

                    using var ticketCacheHelperDeserialization = new CacheHelper<TicketHolder>();
                    var result = ticketCacheHelperDeserialization.Deserialize();
                    CartCacheHelper.Clear();
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Order", $"Your reservation is created", "Ok"));
                }
            }
        }

        public static ReadOnlyMemory<T> CreateTables<T>(TableByteHolder[] tableHolders) where T : ICacheTable<TimeSpan> 
        {
            int length = tableHolders.Length;
            Memory<T> tables = new T[length];

            for (int i = 0; i < length; i++)
            {
                var currentType = typeof(T);
                tables.Span[i] = (T)read.MakeGenericMethod(currentType).Invoke(null, new object[] { tableHolders[i].TableBytes });
            }
            return tables;
        }

        private bool TryGetDefaultOrders(out ProductHolder[] defaultOrders, ProductHolder[] orders)
        {
            var defaultOrdersList = new List<ProductHolder>();
            using var cartCacheManager = new CartCacheHelper();

            var cartProducts = cartCacheManager.Deserialize();
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].Amount == 0)
                    defaultOrdersList.Add(cartProducts[i]);
            }
            defaultOrders = defaultOrdersList.ToArray();
            return defaultOrders.Length > 0;
        }

        public static async Task RequestCacheTablesPacketAsync(params ICacheTable<TimeSpan>[] cacheFiles)
        {
            for (int i = 0; i < cacheFiles.Length; i++)
            {
                var cacheTable = new CacheProductsTablePacket();
                var currentDirectoryType = cacheFiles[i].GetType();
                var connectionKeys = createConnectioKeys.MakeGenericMethod(currentDirectoryType);

                cacheTable.CacheTables = ((ReadOnlyMemory<CacheConnection>)connectionKeys.Invoke(null, Array.Empty<object>())).ToArray();
                cacheTable.TableType = cacheFiles[i].GetType();
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await SendDataAsync(data), DatagramHelper.WriteDatagram(cacheTable));
            }
        }

        public static async Task SendDataAsync(byte[] data) 
        {
            await staticHolder.ServerSocket.SendAsync(data, SocketFlags.None);
        }
    }
}
