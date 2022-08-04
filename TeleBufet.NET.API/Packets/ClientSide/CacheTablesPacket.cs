using DatagramsNet.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class CacheConnection
    {
        public int Id { get; set; }

        public TimeSpan Key { get; set; }

        public CacheConnection() { }

        public CacheConnection(ITable table, TimeSpan key) 
        {
            Id = table.Id;
            Key = key;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class CacheTablesPacket
    {
        [Field(0)]
        public int Id { get; set; } = 23;

        [Field(1)]
        public CacheConnection[] CacheProducts { get; set; }

        [Field(2)]
        public CacheConnection[] CacheCategories { get; set; }
    }
}