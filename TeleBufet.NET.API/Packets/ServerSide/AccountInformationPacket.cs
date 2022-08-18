using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class AccountInformationPacket
    {
        [Field(0)]
        public int Id { get; set; } = 19;

        [Field(1)]
        public int Indetificator { get; set; }

        [Field(2)]
        public int Karma { get; set; }
    }
}
