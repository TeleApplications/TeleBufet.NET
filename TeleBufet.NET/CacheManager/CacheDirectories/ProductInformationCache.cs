namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal sealed class ProductInformationCache : CacheFile
    {
        public override string FileName => "ProductInformationCacheTwentysssssss";
        public static TimeSpan LastChanges { get; set; }

        public ProductInformationCache() : base() { }
    }
}
