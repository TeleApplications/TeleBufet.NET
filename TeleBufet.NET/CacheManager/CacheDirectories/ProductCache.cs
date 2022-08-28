using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(ProductTable))]
    internal sealed class ProductCache : CacheFile
    {
        public override string FileName => "CacheProductsTwoTwelvesssssssssssssssssssssssssssss";

        public static TimeSpan LastChanges { get; set; }

        public ProductCache() : base() { }
    }
}
