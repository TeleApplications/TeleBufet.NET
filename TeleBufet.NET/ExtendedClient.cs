using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.Pages.ProductPage;

namespace TeleBufet.NET
{
    internal sealed class ExtendedClient : ServerManager
    {
        public override int PortNumber => 1111;
        public IPAddress ClientAddress { get; set; }

        public static Socket ?ClientSocket { get; private set; }

        //TODO: In a future every request packet is going to have a proper interface
        public static TimeSpan lastRequest { get; private set; }

        private static ExtendedClient staticHolder;

        public ExtendedClient(string name, IPAddress clientAddress, IPAddress serverAddress) : base(name, serverAddress) //TODO: constructor only socket of server
        {
            ClientAddress = serverAddress;
            this.ServerSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(ServerSocket);
            staticHolder = this;

            var handShakePacket = new TwoWayHandshake() {Message = "Test message"};
            DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(handShakePacket));
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshake newDatagram) 
                Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Reciever", "You recieve back a new HandShakePacket", "Done")); //TODO: Better implementation, however it's just for testing
            if (datagram is AccountInformationPacket newAccountPacket) 
            {
                MainProductPage.User = new UserTable() {Id = newAccountPacket.Indetificator, Karma = newAccountPacket.Karma};
                await RequestCacheTablesPacketAsync();
            }

            //TODO: Create pure generics solution for this type of packets
            //We hope that this will be possible in the next update of Datagrams.NET
            if (datagram is UncachedTablesPacket newUncachedTablesPacket) 
            {
                CacheTables<ProductTable, ProductCache>(newUncachedTablesPacket.Products);
                CacheTables<CategoryTable, CategoryCache>(newUncachedTablesPacket.Categories);
            }
            if (datagram is ProductsInformationPacket newProductsInformationPacket) 
            {
                CacheTables<ProductInformationTable, ProductInformationCache>(newProductsInformationPacket.ProductsInfromations);
                object lockObject = new object();
                lock (lockObject) 
                {
                    lastRequest = DateTime.Now.TimeOfDay;
                }
            }

            if (datagram is OrderTransmitionPacket newTransmitionPacket) 
            {
                using var cartCacheHelper = new CartCacheHelper();
                using var productCacheHelper = new TableCacheHelper<ProductTable, ProductCache>();
                using var ticketCacheHelperSerialization = new CacheHelper<TicketHolder, int, ReservationTicketCache>();

                var products = productCacheHelper.Deserialize();
                if (TryGetDefaultOrders(out ProductHolder[] defaultProducts, newTransmitionPacket.Products))
                {
                    for (int i = 0; i < defaultProducts.Length; i++)
                    {
                        int index = defaultProducts[i].Id;
                        Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Out of stock", $"Sorry, but product {products[index].Name} is already", "Ok"));

                        //This cause that this product will be removed from shopping cart cache
                        cartCacheHelper.CacheValue = new ProductHolder(index, 0);
                        cartCacheHelper.Serialize();
                    }
                }
                else 
                {
                    ticketCacheHelperSerialization.CacheValue = new TicketHolder(newTransmitionPacket.Indetifactor, newTransmitionPacket.Products, newTransmitionPacket.ReservationTimeId, newTransmitionPacket.TotalPrice);
                    ticketCacheHelperSerialization.Serialize();

                    using var ticketCacheHelperDeserialization = new CacheHelper<TicketHolder, int, ReservationTicketCache>();
                    var result = ticketCacheHelperDeserialization.Deserialize();
                    CartCacheHelper.Clear();
                    Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Order", $"Your reservation is created", "Ok"));
                }
            }
        }

        private bool TryGetDefaultOrders(out ProductHolder[] defaultOrders, ProductHolder[] orders)
        {
            var defaultOrdersList = new List<ProductHolder>();
            using var cartCacheHelper = new CartCacheHelper();
            var cartProducts = cartCacheHelper.Deserialize();
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i].Amount == 0)
                    defaultOrdersList.Add(cartProducts[i]);
            }
            defaultOrders = defaultOrdersList.ToArray();
            return defaultOrders.Length > 0;
        }

        public static async Task RequestCacheTablesPacketAsync()
        {
            var cacheTable = new CacheTablesPacket();
            cacheTable.CacheProducts = TryGetCacheConnectionKeys<ProductTable, ProductCache>(out IEnumerable<CacheConnection> productsConnection) ? productsConnection.ToArray() : cacheTable.CacheProducts;
            cacheTable.CacheCategories = TryGetCacheConnectionKeys<CategoryTable, CategoryCache>(out IEnumerable<CacheConnection> categoryConnection) ? categoryConnection.ToArray() : cacheTable.CacheCategories;
            await DatagramHelper.SendDatagramAsync(async (byte[] data) => await SendDataAsync(data), DatagramHelper.WriteDatagram(cacheTable));
        }

        public static async Task SendDataAsync(byte[] data) 
        {
            await staticHolder.ServerSocket.SendAsync(data, SocketFlags.None);
        }

        private void CacheTables<T, TDirectory>(T[] newTables) where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
        {
            using var tableSerialization = new TableCacheHelper<T, TDirectory>();
            for (int i = 0; i < newTables.Length; i++)
            {
                tableSerialization.CacheValue = (T)newTables[i];
                tableSerialization.Serialize();
            }
		    using var informationTableCacheManager = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
            var data = informationTableCacheManager.Deserialize();
        }

        private static bool TryGetCacheConnectionKeys<T, TDirectory>(out IEnumerable<CacheConnection> connections) where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
        {
            connections = GetCacheConnectionKeys<T, TDirectory>();
            return connections.Count() > 0;
        }

        private static IEnumerable<CacheConnection> GetCacheConnectionKeys<T, TDirectory>() where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new()
        {
            using var cacheManager = new CacheHelper<T, TimeSpan, TDirectory>();
            var tables = cacheManager.Deserialize();
            for (int i = 0; i < tables.Length; i++)
            {
                var connectionKey = new CacheConnection(tables[i], tables[i].Key);
                yield return connectionKey;
            }
        }

        protected override async Task<ClientDatagram> StartRecievingAsync()
        {
            var memory = new byte[4096];
            var data = await this.ServerSocket.ReceiveAsync(memory, SocketFlags.None);
            return new ClientDatagram() { Client = default, Datagram = memory };
        }
    }
}
