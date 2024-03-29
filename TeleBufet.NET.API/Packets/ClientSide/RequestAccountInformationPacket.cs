﻿using DatagramsNet.Attributes;
using System.Runtime.InteropServices;

namespace TeleBufet.NET.API.Packets.ClientSide
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class RequestAccountInformationPacket
    {
        [Field(0)]
        public int Id { get; set; } = 38;
    }
}
