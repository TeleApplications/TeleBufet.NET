using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.Pages.ProductPage;

namespace TeleBufet.NET
{
    internal sealed class ExtendedClient : ServerManager
    {
        public override int PortNumber => 1111;
        public IPAddress ClientAddress { get; set; }

        public static Socket ?ClientSocket { get; private set; }
        public static TimeSpan lastRequest { get; private set; }

        private static ExtendedClient staticHolder;
        private static readonly MethodInfo createConnectioKeys = typeof(TableCacheBuilder).GetMethod(nameof(TableCacheBuilder.GetCacheConnectionKeys));
        private static readonly MethodInfo createCacheTables = typeof(TableCacheBuilder).GetMethod(nameof(TableCacheBuilder.CacheTables));

        public ExtendedClient(string name, IPAddress clientAddress, IPAddress serverAddress) : base(name, serverAddress)
        {
            ClientAddress = serverAddress;
            this.ServerSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(ServerSocket);
            staticHolder = this;

            var handShakePacket = new TwoWayHandshake() {Message = "Handshake message"};
            DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(handShakePacket));
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshake newDatagram) 
                Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Reciever", "You recieve back a new HandShakePacket", "Done")); //TODO: Better implementation, however it's just for testing
            if (datagram is AccountInformationPacket newAccountPacket) 
                MainProductPage.User = new UserTable() {Id = newAccountPacket.Indetificator, Karma = newAccountPacket.Karma};

            if (datagram is UncachedTablesPacket newUncachedTablesPacket)
            {
                createCacheTables.MakeGenericMethod(newUncachedTablesPacket.TableType).Invoke(null, new object[] { newUncachedTablesPacket.CacheTables });
                object lockObject = new object();
                lock (lockObject) 
                {
                    lastRequest = DateTime.Now.TimeOfDay;
                }
            }

            if (datagram is ProductsInformationPacket newProductsInformationPacket) 
            {
                TableCacheBuilder.CacheTables<ProductInformationTable>(newProductsInformationPacket.ProductsInfromations);
                object lockObject = new object();
                lock (lockObject) 
                {
                    lastRequest = DateTime.Now.TimeOfDay;
                }
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

        public static async Task RequestCacheTablesPacketAsync(params CacheFile[] cacheFiles)
        {
            for (int i = 0; i < cacheFiles.Length; i++)
            {
                var cacheTable = new CacheProductsTablePacket();
                var currentDirectoryType = cacheFiles[i].GetType();
                var connectionKeys = createConnectioKeys.MakeGenericMethod(currentDirectoryType);

                cacheTable.CacheTables = (CacheConnection[])connectionKeys.Invoke(null, Array.Empty<object>());
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await SendDataAsync(data), DatagramHelper.WriteDatagram(cacheTable));
            }
        }

        public static async Task SendDataAsync(byte[] data) 
        {
            await staticHolder.ServerSocket.SendAsync(data, SocketFlags.None);
        }

        protected override async Task<ClientDatagram> StartRecievingAsync()
        {
            var memory = new byte[4096];
            await this.ServerSocket.ReceiveAsync(memory, SocketFlags.None);
            return new ClientDatagram() { Client = default, Datagram = memory };
        }
    }
}
