
namespace TeleBufet.NET.CacheManager.Interfaces
{
    internal interface ICacheDirectory
    {
        public string SystemCacheDirectory { get; }

        public string FileName { get; }

        public Stream CacheFileStream { get; set; } 
    }
}
