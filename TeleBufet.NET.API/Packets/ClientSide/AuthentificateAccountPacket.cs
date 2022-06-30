using DatagramsNet;
using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class AuthentificateAccountPacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 18;

        [MarshalAs(UnmanagedType.LPStruct)]
        [Field(1)]
        public NormalAccount Account;
    }
}
