using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(CategoryTable))]
    internal sealed class CategoryCache : CacheFile
    {
        public override string FileName => "CacheCategoriesTwelvesssssssssssssssss";
        public static TimeSpan LastChanges { get; set; }

        public CategoryCache() : base() { }
    }
}
