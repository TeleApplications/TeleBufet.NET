using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CacheConnection<T, TKey> where T : ITable, ICache<TKey> where TKey : struct 
    {
        public int Id { get; }

        public TKey Key { get; }

        public CacheConnection(T cacheData) 
        {
            Id = cacheData.Id;
            Key = cacheData.Key;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class CacheTablesPacket : IDatagram 
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 23;

        [Field(1)]
        public Memory<CacheConnection<ProductTable, TimeSpan>> CacheProducts = new();

        [Field(2)]
        public Memory<CacheConnection<CategoryTable, TimeSpan>> CacheCategories = new();
    }
}
