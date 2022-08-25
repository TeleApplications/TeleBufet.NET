﻿using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(TicketHolder))]
    internal class ReservationTicketCache : CacheFile
    {
        public override string FileName => "CacheReservationTicketssssssssssssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public ReservationTicketCache() : base() { }
    }
}
