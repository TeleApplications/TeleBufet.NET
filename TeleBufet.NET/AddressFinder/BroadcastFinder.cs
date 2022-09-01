using System.Net;
using TeleBufet.NET.AddressFinder.Clients;
using TeleBufet.NET.AddressFinder.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;

namespace TeleBufet.NET.AddressFinder
{
    //TODO: In the future this finder is going to have proper find of server ip address
    //by computing current address with subnet mask
    internal sealed class BroadcastFinder : IAddressFinder<AddressClient>
    {
        private static readonly TwoWayHandshake handshakePacket = new() { Message = nameof(BroadcastFinder) };

        public IPAddress BroadcastAddress { get; }
        public AddressClient ResponseClient { get; private set; }

        public BroadcastFinder(IPAddress address)
        {
            BroadcastAddress = address;
        }

        public async Task<IPAddress> StartFindingAsync() 
        {
            ResponseClient = new("ResponseClinet", BroadcastAddress);
            await ResponseClient.PingAsync(handshakePacket);
            return null;
        }
    }
}
