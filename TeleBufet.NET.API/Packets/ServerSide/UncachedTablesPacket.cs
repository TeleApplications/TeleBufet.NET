using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TableHolder 
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ProductTable[] Products;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public CategoryTable[] Categories;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class UncachedTablesPacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 16;

        [MarshalAs(UnmanagedType.LPStruct)]
        [Field(1)]
        public TableHolder Tables;
    }
}