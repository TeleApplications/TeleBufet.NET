using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    internal sealed class CacheCategoriesTablePacket
    {
        [Field(0)]
        public int Id { get; set; } = 45;

        [Field(1)]
        public Memory<CacheConnection> CacheTables { get; set; } = new Memory<CacheConnection>();
    }
}
