using DatagramsNet.Attributes;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class ProductsInformationPacket
    {
        [Field(0)]
        public int Id { get; set; } = 30;

        [Field(1)]
        public ProductInformationTable[] ProductsInfromations { get; set; }
    }
}
