using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(CategoryTable))]
    internal sealed class CategoryCache : CacheFile
    {
        public override string FileName => "FirstCategoryCache";

        public CategoryCache() : base() { }
    }
}
