using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ServerSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class AccountInformationPacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 19;

        [Field(1)]
        public int Karma;
    }
}
