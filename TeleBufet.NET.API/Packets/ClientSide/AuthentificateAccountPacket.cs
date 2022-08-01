using DatagramsNet;
using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class AuthentificateAccountPacket
    {
        [Field(0)]
        public int Id { get; set; } = 18;

        [Field(1)]
        public NormalAccount Account { get; set; }
    }
}
