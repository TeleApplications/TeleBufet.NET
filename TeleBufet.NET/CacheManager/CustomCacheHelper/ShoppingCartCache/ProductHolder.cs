using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ProductHolder : ITable, ICache<TimeSpan>
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public TimeSpan Key { get; set; }

        public ProductHolder(int productId, int amount) 
        {
            Id = productId;
            Amount = amount;
        }
    }
}
