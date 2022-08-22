using DatagramsNet;
using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class OrderTransmitionPacket
    {
        [Field(0)]
        public int Id { get; set; } = 35;

        [Field(1)]
        public ProductHolder[] Products { get; set; } = new ProductHolder[1];

        [Field(2)]
        public int ReservationTimeId { get; set; }

        [Field(3)]
        public double TotalPrice { get; set; }

        [Field(4)]
        public int Indetifactor { get; set; }

        [Field(5)]
        public string StringDateTime { get; set; }
    }
}
