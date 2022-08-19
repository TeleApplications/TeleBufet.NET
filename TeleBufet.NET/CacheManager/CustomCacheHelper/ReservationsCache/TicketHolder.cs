using DatagramsNet;
using System.Runtime.InteropServices;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache
{
    internal enum TicketState 
    {
        Expired = 0,
        Unexpired = 1
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal sealed class TicketHolder : ICache<int>
    {
        public int Id { get; set; }
        public ProductHolder[] Products { get; set; } = new ProductHolder[1];
        public int Key { get; set; }
        public double FinalPrice { get; set; }
        public bool IsExpired { get; set; } = false;

        public TicketHolder() { }
        public TicketHolder(int id, ProductHolder[] products, int reservationTime, double finalPrice)
        {
            Id = id;
            Products = products;
            Key = reservationTime;
            FinalPrice = finalPrice;
            IsExpired = GetCurrentState(IsExpired);
        }

        private bool GetCurrentState(bool currentState)
        {
            if (currentState)
                return currentState; 

            int start = (8 * 60);

            var currentTime = DateTime.Now.TimeOfDay;
            var ticketTime = TimeSpan.FromMinutes(start + (Key * 60));

            currentState = currentTime < ticketTime;
            return !(currentState);
        }
    }
}
