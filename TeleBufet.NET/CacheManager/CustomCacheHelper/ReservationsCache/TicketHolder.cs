using DatagramsNet;
using System.Globalization;
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
    internal sealed class TicketHolder : ICacheTable<int>
    {
        public int Id { get; set; }
        public ProductHolder[] Products { get; set; } = new ProductHolder[1];
        public int Key { get; set; }
        public double FinalPrice { get; set; }
        public bool IsExpired { get; set; } = false;
        public string StringDateTime { get; set; }

        public TicketHolder() { }
        public TicketHolder(int id, ProductHolder[] products, int reservationTime, double finalPrice, string dateTime)
        {
            Id = id;
            Products = products;
            Key = reservationTime;
            FinalPrice = finalPrice;
            StringDateTime = dateTime;
            var currentDateTime = DateTime.ParseExact(dateTime, "MM.dd.yyyy", CultureInfo.InvariantCulture);
            IsExpired = GetCurrentState(Key, currentDateTime);
        }

        public static ReadOnlyMemory<TicketHolder> GetCurrentTickets(TicketHolder[] tickets) 
        {
            Memory<TicketHolder> currentTickets = new TicketHolder[tickets.Length];
            int index = 0;

            for (int i = 0; i < tickets.Length; i++)
            {
                var currentTicket = tickets[i];
                if (GetCurrentState(currentTicket.Key, DateTime.ParseExact(currentTicket.StringDateTime, "MM.dd.yyyy", CultureInfo.InvariantCulture))) 
                {
                    currentTickets.Span[i] = currentTicket;
                    index++;
                }
            }
            return currentTickets[0..(index)];
        }

        public static bool GetCurrentState(int schoolBreak, DateTime currentDateTime)
        {
            int start = (8 * 60);

            var currentTime = DateTime.UtcNow.TimeOfDay;
            var ticketTime = TimeSpan.FromMinutes(start + (schoolBreak * 60));

            return (currentTime < ticketTime) && currentDateTime.Date >= DateTime.UtcNow.Date;
        }
    }
}
