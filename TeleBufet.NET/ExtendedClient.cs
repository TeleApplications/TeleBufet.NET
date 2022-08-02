using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET
{
    internal sealed class ExtendedClient : ServerManager
    {
        public override int PortNumber => 1111;

        public IPAddress ClientAddress { get; set; }
        public static Socket ?ClientSocket { get; private set; }

        public ExtendedClient(string name, IPAddress clientAddress, IPAddress serverAddress) : base(name, serverAddress) //TODO: constructor only socket of server
        {
            ClientAddress = serverAddress;
            this.ServerSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(ServerSocket);
            ClientSocket = ServerSocket;
            var handShakePacket = new TwoWayHandshake() {Message = "Test message"};
            DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(handShakePacket));
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is TwoWayHandshake newDatagram) 
            {
                Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Reciever", "You recieve back a new HandShakePacket", "Done")); //TODO: Better implementation, however it's just for testing
            }

            if (datagram is AccountInformationPacket newAccountPacket) 
            {
                var cacheTable = new CacheTablesPacket();
                cacheTable.CacheProducts = TryGetCacheConnectionKeys<ProductTable, ProductCache>(out IEnumerable<CacheConnection> productsConnection) ? productsConnection.ToArray() : cacheTable.CacheProducts;
                cacheTable.CacheCategories = TryGetCacheConnectionKeys<ProductTable, ProductCache>(out IEnumerable<CacheConnection> categoryConnection) ? categoryConnection.ToArray() : cacheTable.CacheCategories;
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.ServerSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(cacheTable));
            }
            if (datagram is UncachedTablesPacket newUncachedTablesPacket) 
            {
                //TODO: Do a serialization into cache directory with CacheHelper.cs
                //Then create into CacheHelper.cs new method that allow you to replace old serailization to new one with same id
                CacheTables<ProductTable, ProductCache>(newUncachedTablesPacket.Products);
                CacheTables<CategoryTable, CategoryCache>(newUncachedTablesPacket.Categories);
            }
        }

        private void CacheTables<T, TDirectory>(T[] newTables) where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
        {
            using var tableSerialization = new TableCacheHelper<T, TDirectory>();
            for (int i = 0; i < newTables.Length; i++)
            {
                tableSerialization.CacheValue = (T)newTables[i];
                tableSerialization.Serialize();
            }
        }

        private bool TryGetCacheConnectionKeys<T, TDirectory>(out IEnumerable<CacheConnection> connections) where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
        {
            connections = GetCacheConnectionKeys<T, TDirectory>();
            return connections.Count() > 0;
        }

        private IEnumerable<CacheConnection> GetCacheConnectionKeys<T, TDirectory>() where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new()
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
