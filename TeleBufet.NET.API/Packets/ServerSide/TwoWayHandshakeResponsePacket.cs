
using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ServerSide
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class TwoWayHandshakeResponsePacket
    {
        [Field(0)]
        public int Id { get; set; } = 41;

        [Field(1)]
        public string IpAddress { get; set; }
    }
}
