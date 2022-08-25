using DatagramsNet.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class CacheConnection
    {
        public int Id { get; set; }
        public TimeSpan Key { get; set; }

        public CacheConnection() { }

        public CacheConnection(ICacheTable<TimeSpan> table) 
        {
            Id = table.Id;
            Key = table.Key;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class CacheProductsTablePacket
    {
        [Field(0)]
        public int Id { get; set; } = 23;

        [Field(1)]
        public Memory<CacheConnection> CacheTables { get; set; } = new Memory<CacheConnection>();

        [Field(2)]
        public Type TableType { get; set; }
    }
}