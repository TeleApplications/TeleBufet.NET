using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class TwoWayHandshake
    {
        [Field(0)]
        public int Id { get; set; } = 26;

        [Field(1)]
        public string Message { get; set; }
    }
}
