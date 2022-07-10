using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    //TODO: This needs to be a generic struct, however in this current state we have problem with getting size of struct
    //with generics fields. And also without using unsafe implementation
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    public struct CacheConnection<T> where T : ITable, ICache<TimeSpan>
    {
        public int Id { get; }

        public TimeSpan Key;

        public CacheConnection(ITable table) 
        {
            var tableCache = (ICache<TimeSpan>)table;
            Id = table.Id;
            Key = tableCache.Key;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class CacheConnectionHolder 
    {

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public CacheConnection<ProductTable>[] CacheProducts;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public CacheConnection<CategoryTable>[] CacheCategories;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class CacheTablesPacket : IDatagram 
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 23;

        [MarshalAs(UnmanagedType.LPStruct)]
        [Field(1)]
        public CacheConnectionHolder ConnectioHolder;
    }
}