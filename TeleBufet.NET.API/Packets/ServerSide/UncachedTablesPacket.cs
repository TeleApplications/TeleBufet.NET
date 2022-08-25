using DatagramsNet.Attributes;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class UncachedTablesPacket
    {

        [Field(0)]
        public int Id { get; set; } = 28;

        [Field(1)]
        public Memory<ICacheTable<TimeSpan>> CacheTables { get; set; } = new Memory<ICacheTable<TimeSpan>>();

        [Field(2)]
        public Type TableType { get; set; }
    }
}