using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ServerSide
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class TableByteHolder 
    {
        public byte[] TableBytes { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class UncachedTablesPacket
    {
        [Field(0)]
        public int Id { get; set; } = 49;

        [Field(1)]
        public TableByteHolder[] TableHolders { get; set; }

        [Field(2)]
        public Type TableType { get; set; }
    }
}