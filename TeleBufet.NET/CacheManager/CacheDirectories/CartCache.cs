
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{

    internal sealed class CartCache : CacheFile
    {
        public override string FileName => "CartCachessssssssssssssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public CartCache() : base() { }
    }
}
