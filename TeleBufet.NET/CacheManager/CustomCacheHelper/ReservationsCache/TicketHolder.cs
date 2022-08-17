using DatagramsNet;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct TicketHolder : ICache<TimeSpan>
    {
        public ProductHolder[] Products { get; }
        public TimeSpan Key { get; set; }
        public double FinalPrice { get; }

        public TicketHolder(ProductHolder[] products, TimeSpan reservationTime, double finalPrice)
        {
            Products = products;
            Key = reservationTime;
            FinalPrice = finalPrice;
        }
    }
}
