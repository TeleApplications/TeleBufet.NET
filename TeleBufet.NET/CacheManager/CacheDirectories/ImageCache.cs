using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(ImageTable))]
    internal sealed class ImageCache : CacheFile
    {
        private static ReadOnlyMemory<ImageTable> cacheImages;
        private static TimeSpan CurrentChanges { get; set; }

        public static TimeSpan LastChanges { get; set; }

        public override string FileName => "CacheImages";

        public ImageCache() : base() { }

        public static ReadOnlyMemory<ImageTable> GetCacheTables() 
        {
            if (cacheImages.IsEmpty || LastChanges != CurrentChanges) 
            {
                using var tables = new TableCacheHelper<ImageTable>();
                cacheImages = tables.Deserialize();
                LastChanges = CurrentChanges;
            }

            return cacheImages;
        }
    }
}
