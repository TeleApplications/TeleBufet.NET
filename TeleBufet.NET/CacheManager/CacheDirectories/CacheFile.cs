using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal abstract class CacheFile : ICacheDirectory
    {
        public string SystemCacheDirectory => FileSystem.CacheDirectory;
        public abstract string FileName { get; }
        public Stream CacheFileStream { get; set; }

        public CacheFile() 
        {
            var path = $"{SystemCacheDirectory}{FileName}";
            CacheFileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }
    }
}
