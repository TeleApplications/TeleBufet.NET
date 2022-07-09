using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    //[Packet]
    public sealed class UncachedTablesPacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 16;

        [MarshalAs(UnmanagedType.ByValArray)]
        [Field(1)]
        public ProductTable[] Products = new ProductTable[1];

        [MarshalAs(UnmanagedType.ByValArray)]
        [Field(2)]
        public CategoryTable[] Categories = new CategoryTable[1];
    }
}
