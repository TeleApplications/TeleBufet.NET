using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.API.Packets.ServerSide;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

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
            this.serverSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(serverSocket);
            ClientSocket = serverSocket;
            var handShakePacket = new HandShakePacket(new ShakeMessage() {IdMessage = 17, Message = "Client Message with id 17" });
            Task.Run(async() => await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(handShakePacket)));
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandShakePacket newDatagram) 
            {
                Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Reciever", "You recieve back a new HandShakePacket", "Done")); //TODO: Better implementation, however it's just for testing
                //throw new Exception("Packet was recieved from server");
            }
            if (datagram is AccountInformationPacket newAccountPacket) 
            {
                var cacheTable = new CacheTablesPacket() 
                {
                    CacheProducts = GetCacheConnectionKeys<ProductTable, ProductCache>().ToArray(),
                    CacheCategories = GetCacheConnectionKeys<CategoryTable, CategoryCache>().ToArray(),
                };
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(cacheTable));
            }
        }

        private IEnumerable<CacheConnection<T, TimeSpan>> GetCacheConnectionKeys<T, TDirectory>()  where T : struct, ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new()
        {
            using var cacheManager = new CacheHelper<T, TimeSpan, TDirectory>();
            var tables = cacheManager.Deserialize();
            for (int i = 0; i < tables.Length; i++)
            {
                var connectionKey = new CacheConnection<T, TimeSpan>(tables[i]);
                yield return connectionKey;
            }
        }

        protected override async Task<ClientDatagram> StartRecieving()
        {
            var memory = new byte[4096];
            var data = await this.serverSocket.ReceiveAsync(memory, SocketFlags.None);
            return new ClientDatagram() { Client = default, Datagram = memory };
        }
    }
}
