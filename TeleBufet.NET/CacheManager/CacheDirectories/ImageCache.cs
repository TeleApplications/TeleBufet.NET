using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    [CacheTable(typeof(ImageTable))]
    internal sealed class ImageCache : CacheFile
    {
        private static ReadOnlyMemory<ImageTable> cacheImages;
        private static TimeSpan lastChanges;

        //TODO: Add in section of recieving image packet a new CurrentChanges timespan
        public static TimeSpan CurrentChanges { get; set; }


        public override string FileName => "FirstImageCache";

        public ImageCache() : base() { }

        public static ReadOnlyMemory<ImageTable> GetCacheTables() 
        {
            if (cacheImages.IsEmpty || lastChanges != CurrentChanges) 
            {
                using var tables = new TableCacheHelper<ImageTable>();
                cacheImages = tables.Deserialize();
                lastChanges = CurrentChanges;
            }

            return cacheImages;
        }
    }
}
