using DatagramsNet;
using DatagramsNet.Datagrams.NET.Prefixes;
using DatagramsNet.Interfaces;
using DatagramsNet.NET.Logger;
using System.Net;
using System.Net.Sockets;

namespace TeleBufet.Server
{
    internal sealed class ServerSender : UdpClient
    {
        public string Name { get; set; }

        public int Port { get; private set; }

        public ServerSender(string name, int port) 
        {
            Name = name;
            Port = port;

        }

        public async Task SendDatagramAsync<T>(IPAddress clientAddress, T datagram) where T : IDatagram 
        {
            //This is only for testing usage
            if(await PingClientAsync(clientAddress))
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await SendAsync(data), DatagramHelper.WriteDatagram(datagram));
        }

        public async Task<bool> PingClientAsync(IPAddress clientAddress) 
        {
            this.Connect(clientAddress, Port);
            if (Available != 0)
            {
                var handShakeMessage = new HandShakePacket(new ShakeMessage() { IdMessage = 25, Message = "ClientPing susceede".ToCharArray() });
                await DatagramHelper.SendDatagramAsync(async (byte[] data) => await SendAsync(data), DatagramHelper.WriteDatagram(handShakeMessage));
                return true;
            }
            await ServerLogger.Log<ErrorPrefix>("Client is not connected", TimeFormat.HALF);
            return false;
        }
    }
}
