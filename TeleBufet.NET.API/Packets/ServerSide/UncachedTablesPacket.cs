using DatagramsNet.Attributes;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class UncachedTablesPacket
    {

        [Field(0)]
        public int Id { get; set; } = 16;

        [Field(1)]
        public ProductTable[] Products { get; set; }

        [Field(2)]
        public CategoryTable[] Categories { get; set; }
    }
}