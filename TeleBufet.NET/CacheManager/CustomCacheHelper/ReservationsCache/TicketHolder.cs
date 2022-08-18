using DatagramsNet;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class TicketHolder : ICache<TimeSpan>
    {
        public int Id { get; set; }
        public ProductHolder[] Products { get; set; } = new ProductHolder[1];
        public TimeSpan Key { get; set; } = TimeSpan.Zero;
        public double FinalPrice { get; set; }

        public TicketHolder() { }
        public TicketHolder(int id, ProductHolder[] products, TimeSpan reservationTime, double finalPrice)
        {
            Id = id;
            Products = products;
            Key = reservationTime;
            FinalPrice = finalPrice;
        }
    }
}
