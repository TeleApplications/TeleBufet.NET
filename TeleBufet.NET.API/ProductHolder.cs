using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class ProductHolder : ITable, ICache<TimeSpan>
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public TimeSpan Key { get; set; } = TimeSpan.Zero;

        public ProductHolder() { }
        public ProductHolder(int productId, int amount) 
        {
            Id = productId;
            Amount = amount;
        }
    }
}
