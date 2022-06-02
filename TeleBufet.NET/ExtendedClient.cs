using DatagramsNet;
using System.Net;
using System.Net.Sockets;

namespace TeleBufet.NET
{
    internal sealed class ExtendedClient : ServerManager
    {
        public override int PortNumber => 1111;

        public IPAddress ClientAddress { get; set; }

        public ExtendedClient(string name, IPAddress clientAddress, IPAddress serverAddress) : base(name, serverAddress) //TODO: constructor only socket of server
        {
            ClientAddress = serverAddress;
            this.serverSocket.Connect((EndPoint)new IPEndPoint(clientAddress, PortNumber));
            this.UdpReciever = new UdpReciever(serverSocket);
            var handShakePacket = new HandShakePacket(new ShakeMessage() {IdMessage = 17, Message = "Client Message with id 17".ToCharArray() });
            DatagramHelper.SendDatagramAsync(async (byte[] data) => await this.serverSocket.SendAsync(data, SocketFlags.None), DatagramHelper.WriteDatagram(handShakePacket));
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandShakePacket newDatagram) 
            {
                Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Reciever", "You recieve back a new HandShakePacket", "Done")); //TODO: Better implementation, however it's just for testing
                throw new Exception("Packet was recieved from server");

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
