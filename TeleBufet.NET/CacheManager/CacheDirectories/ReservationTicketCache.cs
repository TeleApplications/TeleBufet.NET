
namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal class ReservationTicketCache : CacheFile
    {
        public override string FileName => "CacheReservationTicketssssssssssssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public ReservationTicketCache() : base() { }
    }
}
