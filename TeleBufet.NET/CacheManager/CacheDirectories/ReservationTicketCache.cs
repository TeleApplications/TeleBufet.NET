
namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal class ReservationTicketCache : CacheFile
    {
        public override string FileName => "CacheReservationTicketssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public ReservationTicketCache() : base() { }
    }
}
